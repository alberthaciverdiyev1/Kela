namespace Kela.Application.Validation;

/// <summary>
/// İstek doğrulama hatası. GlobalExceptionHandler bunu 400 Bad Request'e çevirir.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(IReadOnlyDictionary<string, string> errors)
        : base("Doğrulama hatası.")
    {
        Errors = errors;
    }

    /// <summary>Hata veren alan adı → hata mesajı.</summary>
    public IReadOnlyDictionary<string, string> Errors { get; }
}
