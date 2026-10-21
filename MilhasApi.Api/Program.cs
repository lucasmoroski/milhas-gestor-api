using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MilhasApi.Api.Data;
using MilhasApi.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var isDevelopment = builder.Environment.IsDevelopment();

// ---------- Banco de dados (SqlServer local | Postgres em producao) ----------
var provider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection nao configurada.");

if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
{
    // Permite gravar DateTime de qualquer Kind em 'timestamp' (sem fuso), como no SQL Server.
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    var pgConnection = ConnectionStrings.NormalizePostgres(connectionString);
    builder.Services.AddDbContext<MilhasDbContext, MilhasPgDbContext>(options => options.UseNpgsql(pgConnection));
}
else
{
    builder.Services.AddDbContext<MilhasDbContext>(options => options.UseSqlServer(connectionString));
}

// ---------- Segredos obrigatorios fora do desenvolvimento ----------
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"] ?? throw new InvalidOperationException("Jwt:Key nao configurada.");
if (!isDevelopment)
{
    if (jwtKey.Length < 32 || jwtKey.StartsWith("TROQUE", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Defina Jwt__Key com um segredo aleatorio de pelo menos 32 caracteres.");
    if (string.IsNullOrWhiteSpace(builder.Configuration["Crypto:Key"]))
        throw new InvalidOperationException("Defina Crypto__Key (segredo para criptografar senhas dos programas de milhas).");
}

// ---------- Autenticacao ----------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(1),
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAccessScope, AccessScope>();
builder.Services.AddSingleton<ISecretProtector, SecretProtector>();

// ---------- Protecao contra forca bruta no login ----------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", httpContext => RateLimitPartition.GetFixedWindowLimiter(
        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ---------- CORS ----------
const string CorsPolicy = "FrontendPolicy";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Milhas API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Informe: Bearer {seu token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- Migrations + seed (e importacao opcional: --import-csv "<pasta>") ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MilhasDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    await DbInitializer.SeedAsync(db, app.Configuration, logger);

    var importDir = app.Configuration["import-csv"];
    if (!string.IsNullOrWhiteSpace(importDir))
    {
        var protector = scope.ServiceProvider.GetRequiredService<ISecretProtector>();
        await new CsvImporter(db, protector, importDir, Console.Out).RunAsync();
        return;
    }
}

app.UseForwardedHeaders();

if (isDevelopment)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseCors(CorsPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.MapControllers();

app.Run();
