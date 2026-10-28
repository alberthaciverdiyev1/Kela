using Microsoft.Extensions.Configuration;

namespace Kela.Core.Common.Configuration;

/// <summary>
/// Development-dan başqa mühitlərdə secret-lərin default/boş qalmadığını yoxlayır.
/// Səhv konfiqurasiya ilə işə düşmək, açıq açar və default parolla işləməkdən pisdir.
/// </summary>
public static class SecretGuard
{
    private static readonly string[] Placeholders = ["CHANGE_THIS", "please-change-me"];
    private static readonly string[] WeakPasswords = ["admin123", "student123", "password", "12345678"];

    // requireJwt: true YALNIZ JWT verən host üçün (API). Admin cookie auth işlədir.
    public static void EnsureProductionSecrets(
        IConfiguration configuration, bool isDevelopment, bool requireJwt = true)
    {
        if (isDevelopment) return;

        if (requireJwt)
        {
            var jwt = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwt)
                || jwt.Length < 32
                || Placeholders.Any(p => jwt.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    "Jwt:Key must be a strong secret (at least 32 characters) outside Development.");
            }
        }

        var connection = configuration.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connection)
            || connection.Contains("Password=postgres", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Postgres must be configured outside Development.");
        }

        RequirePassword(configuration["Admin:Password"], "Admin:Password");
        RequirePassword(configuration["DefaultAccounts:Student:Password"], "DefaultAccounts:Student:Password");
        RequirePassword(configuration["DefaultAccounts:Admin:Password"], "DefaultAccounts:Admin:Password");
    }

    private static void RequirePassword(string? value, string key)
    {
        if (string.IsNullOrWhiteSpace(value)
            || WeakPasswords.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"{key} must be set to a non-default value outside Development.");
        }
    }
}
