using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LanguageLMS.Core.Common.Email;

/// <summary>SMTP ilə e-poçt göndərir. Smtp:Host boşdursa göndərmir (dev).</summary>
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            logger.LogWarning("SMTP konfiqurasiya olunmayıb — '{To}' ünvanına e-poçt göndərilmədi (subject: {Subject})", to, subject);
            return;
        }

        using var message = new MailMessage(_options.From, to, subject, htmlBody) { IsBodyHtml = true };
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.User, _options.Password)
        };

        await client.SendMailAsync(message, ct);
    }
}
