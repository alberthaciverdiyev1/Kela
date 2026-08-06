namespace Kela.Application.Validation;

/// <summary>
/// İstek doğrulama hatası. GlobalExceptionHandler bunu 400 Bad Request'e çevirir.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyCollection<string> errors)
        : base("Doğrulama hatası.")
    {
        Errors = errors;
    }

    /// <summary>İhlal edilen kurallara ait hata mesajları.</summary>
    public IReadOnlyCollection<string> Errors { get; }
}
