namespace MilhasApi.Api.Data;

public static class ConnectionStrings
{
    /// <summary>
    /// Aceita tanto o formato 'Host=...;Database=...' quanto a URL 'postgresql://user:senha@host/db'
    /// (formato entregue por Neon/Supabase/Render) e devolve uma connection string do Npgsql.
    /// </summary>
    public static string NormalizePostgres(string value)
    {
        if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return value;

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.Trim('/');

        var ssl = "Require";
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        if (string.Equals(query["sslmode"], "disable", StringComparison.OrdinalIgnoreCase)) ssl = "Disable";

        return $"Host={uri.Host};Port={port};Database={database};Username={user};Password={password};SSL Mode={ssl};Trust Server Certificate=true";
    }
}
