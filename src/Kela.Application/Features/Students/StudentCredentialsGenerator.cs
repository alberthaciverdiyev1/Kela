using System.Security.Cryptography;

namespace Kela.Application.Features.Students;

/// <summary>
/// Sistem tarafından üretilen öğrenci giriş bilgileri.
/// Mail, sabit okul domaini + rastgele token ile üretilir; şifre ise
/// kriptografik olarak güvenli rastgele karakterlerden oluşur.
/// Öğretmen bu bilgileri öğrenciye iletir — düz metin yalnızca oluşturma
/// anında response'ta döner, veritabanında yalnızca hash saklanır.
/// </summary>
public static class StudentCredentialsGenerator
{
    /// <summary>Sabit varsayılan domain — ileride site config'den okunabilir.</summary>
    public const string EmailDomain = "kela.edu";

    /// <summary>Mail local-part'ında kullanılan karakterler (küçük harf + rakam).</summary>
    private const string LocalChars = "abcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>Karışıklık yaratmayan karakterler (0/O, 1/l/I yok).</summary>
    private const string PasswordChars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";

    private const int LocalPartLength = 8;
    private const int PasswordLength = 10;

    /// <summary>"s.a1b2c3d4@kela.edu" biçiminde rastgele mail üretir.</summary>
    public static string GenerateEmail()
    {
        var chars = new char[LocalPartLength];
        for (var i = 0; i < chars.Length; i++)
        {
            chars[i] = LocalChars[RandomNumberGenerator.GetInt32(LocalChars.Length)];
        }
        return $"s.{new string(chars)}@{EmailDomain}";
    }

    /// <summary>10 karakterli güçlü ve yazılabilir rastgele şifre üretir.</summary>
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
