using System.Security.Cryptography;
using Kela.Application.Features.Users;

namespace Kela.Infrastructure.Security;

internal sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        var parts = new byte[SaltSize + KeySize];
        Array.Copy(salt, 0, parts, 0, SaltSize);
        Array.Copy(hash, 0, parts, SaltSize, KeySize);

        return Convert.ToBase64String(parts);
    }

    public bool Verify(string password, string passwordHash)
    {
        var parts = Convert.FromBase64String(passwordHash);
        var salt = parts[..SaltSize];
        var expected = parts[SaltSize..];

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
