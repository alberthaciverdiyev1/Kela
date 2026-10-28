using Microsoft.Extensions.Logging;

namespace Edutor.Core.Common.Email;

/// <summary>SMTP konfiqurasiya olunmadıqda e-poçt göndərmir (yalnız loglayır).</summary>
public sealed class NoOpEmailSender(ILogger<NoOpEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        logger.LogInformation("E-poçt göndərilmədi (SMTP konfiqurasiya olunmayıb): {To} / {Subject}", to, subject);
        return Task.CompletedTask;
    }
}
