namespace Kela.Application.Validation;

/// <summary>
/// Basit, harici framework gerektirmeyen doğrulayıcı tabanı.
/// Alt sınıflar <see cref="ValidateCore"/> içinde <see cref="AddError"/> ile hataları toplar;
/// hata varsa <see cref="ValidationException"/> fırlatılır.
/// </summary>
public abstract class Validator<T> : IValidator<T>
{
    public void Validate(T value)
    {
        var errors = new List<string>();
        ValidateCore(value, errors);

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    protected abstract void ValidateCore(T value, List<string> errors);

    protected static void AddError(List<string> errors, string error) => errors.Add(error);
}
