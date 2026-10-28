using System.Security.Cryptography;
using System.Text;
using Edutor.Core.Common.Email;
using Edutor.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Edutor.Core.Modules.Auth.Services;

using OtpCode = Edutor.Core.Modules.Auth.Entities.OtpCode;

public sealed class OtpService(AppDbContext db, IEmailSender emailSender) : IOtpService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    /// <summary>Bir kod üçün icazə verilən səhv cəhd sayı.</summary>
    private const int MaxAttempts = 5;

    public async Task SendAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;

        // Kriptoqrafik generator, 6 rəqəm — 4 rəqəmli kod asanlıqla tapılırdı.
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

        // Yalnız ən son kod keçərlidir: köhnə aktiv kodlar ləğv olunur.
        var previous = await db.OtpCodes
            .Where(x => x.Email == normalized && x.UsedAt == null && x.ExpiresAt > now)
            .ToListAsync(ct);

        foreach (var old in previous) old.UsedAt = now;

        db.OtpCodes.Add(new OtpCode
        {
            Email = normalized,
            Code = code,
            ExpiresAt = now.Add(Lifetime)
        });
        await db.SaveChangesAsync(ct);

        await emailSender.SendAsync(
            normalized,
            "Təsdiq kodu",
            $"<p>E-poçt təsdiq kodunuz: <strong>{code}</strong>. Kod {Lifetime.TotalMinutes:0} dəqiqə qüvvədədir.</p>",
            ct);
    }

    public async Task<bool> VerifyAsync(string email, string code, bool consume = true, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var now = DateTimeOffset.UtcNow;

        var otp = await db.OtpCodes
            .Where(x => x.Email == normalized && x.UsedAt == null && x.ExpiresAt > now)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (otp is null || otp.Attempts >= MaxAttempts) return false;

        var matches = CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(code),
            Encoding.UTF8.GetBytes(otp.Code));

        if (!matches)
        {
            otp.Attempts++;

            // Limit dolubsa kod tamamilə bağlanır — yeni kod istənilməlidir.
            if (otp.Attempts >= MaxAttempts) otp.UsedAt = now;

            await db.SaveChangesAsync(ct);
            return false;
        }

        if (consume)
        {
            otp.UsedAt = now;
            await db.SaveChangesAsync(ct);
        }

        return true;
    }
}
