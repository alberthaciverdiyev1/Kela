namespace Edutor.Core.Modules.Auth.Abstractions;

public interface IOtpService
{
    Task SendAsync(string email, CancellationToken ct = default);

    /// <summary>ensureValid + <paramref name="consume"/> true olduqda kodu istifadə olunmuş kimi işarələyir.</summary>
    Task<bool> VerifyAsync(string email, string code, bool consume = true, CancellationToken ct = default);
}
