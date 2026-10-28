namespace Edutor.Core.Modules.Certificate.Responses;

public record CertificateResponse(
    long Id,
    string Title,
    string TemplateKey,
    string TemplateName,
    string BodyText,
    string? Signature,
    DateTimeOffset CreatedAt);
