using Microsoft.EntityFrameworkCore;
using MilhasApi.Api.Models;

namespace MilhasApi.Api.Data;

public static class DbInitializer
{
    private static readonly (string Codigo, string Nome, string Companhia, string Cor)[] ProgramasPadrao =
    {
        ("smiles", "Smiles", "GOL", "#f97316"),
        ("tudoazul", "TudoAzul", "Azul", "#2563eb"),
        ("azul", "Azul Fidelidade", "Azul", "#0ea5e9"),
        ("latam", "LATAM Pass", "LATAM", "#e11d48"),
        ("livelo", "Livelo", "Multiplos", "#a855f7"),
        ("esfera", "Esfera", "Multiplos", "#16a34a"),
        ("tap", "TAP Miles&Go", "TAP", "#15803d"),
        ("ib", "Iberia Club", "Iberia", "#dc2626"),
        ("caixa", "Caixa", "Multiplos", "#0369a1"),
    };

    /// <summary>
    /// Aplica migrations (se habilitado), garante os programas de fidelidade padrao e,
    /// opcionalmente, cria o admin inicial a partir de Seed:AdminEmail / Seed:AdminPassword.
    /// Nao cria dados de demonstracao.
    /// </summary>
    public static async Task SeedAsync(MilhasDbContext db, IConfiguration config, ILogger logger)
    {
        if (config.GetValue("Database:MigrateOnStartup", true))
            await db.Database.MigrateAsync();

        var existentes = await db.ProgramasFidelidade.Select(p => p.Codigo).ToListAsync();
        foreach (var p in ProgramasPadrao.Where(p => !existentes.Contains(p.Codigo)))
        {
            db.ProgramasFidelidade.Add(new ProgramaFidelidade
            {
                Codigo = p.Codigo, Nome = p.Nome, CompanhiaAerea = p.Companhia, CorHex = p.Cor
            });
        }

        var adminEmail = config["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var adminSenha = config["Seed:AdminPassword"];
        if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminSenha))
        {
            if (!await db.Usuarios.AnyAsync(u => u.Email.ToLower() == adminEmail))
            {
                db.Usuarios.Add(new Usuario
                {
                    Nome = "Administrador",
                    Email = adminEmail,
                    SenhaHash = BCrypt.Net.BCrypt.HashPassword(adminSenha),
                    Perfil = PerfilUsuario.Admin,
                    Ativo = true
                });
                logger.LogInformation("Usuario admin inicial criado ({Email}).", adminEmail);
            }
        }
        else if (!await db.Usuarios.AnyAsync(u => u.Perfil == PerfilUsuario.Admin))
        {
            logger.LogWarning("Nenhum admin existe e Seed:AdminEmail/Seed:AdminPassword nao foram definidos.");
        }

        await db.SaveChangesAsync();
    }
}
