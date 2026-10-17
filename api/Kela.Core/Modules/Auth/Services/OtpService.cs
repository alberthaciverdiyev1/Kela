using Kela.Core.Common.Email;
using Kela.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Core.Modules.Auth.Services;

using OtpCode = Kela.Core.Modules.Auth.Entities.OtpCode;

public sealed class OtpService(AppDbContext db, IEmailSender emailSender) : IOtpService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    public async Task SendAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var code = Random.Shared.Next(1000, 9999).ToString();

        db.OtpCodes.Add(new OtpCode
        {
            Email = normalized,
            Code = code,
            ExpiresAt = DateTimeOffset.UtcNow.Add(Lifetime)
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
            .Where(x => x.Email == normalized && x.Code == code && x.UsedAt == null && x.ExpiresAt > now)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(ct);

        if (otp is null) return false;

        if (consume)
        {
            otp.UsedAt = now;
            await db.SaveChangesAsync(ct);
        }

        return true;
    }
}
