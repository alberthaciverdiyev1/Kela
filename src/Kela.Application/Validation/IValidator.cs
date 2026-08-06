namespace Kela.Application.Validation;

/// <summary>
/// Request doğrulayıcılarının ortak sözleşmesi.
/// Kural ihlali varsa <see cref="ValidationException"/> fırlatır.
/// </summary>
public interface IValidator<in T>
{
    void Validate(T value);
}
