using System.Security.Cryptography;

namespace Kela.Application.Features.Students;

public static class StudentCredentialsGenerator
{
    public const string EmailDomain = "kela.edu";

    private const string LocalChars = "abcdefghijklmnopqrstuvwxyz0123456789";

    private const string PasswordChars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";

    private const int LocalPartLength = 8;
    private const int PasswordLength = 10;

    public static string GenerateEmail()
    {
        var chars = new char[LocalPartLength];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = LocalChars[RandomNumberGenerator.GetInt32(LocalChars.Length)];
        }
        return $"s.{new string(chars)}@{EmailDomain}";
    }

    public static string GeneratePassword()
    {
        var chars = new char[PasswordLength];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = PasswordChars[RandomNumberGenerator.GetInt32(PasswordChars.Length)];
        }
        return new string(chars);
    }
}
