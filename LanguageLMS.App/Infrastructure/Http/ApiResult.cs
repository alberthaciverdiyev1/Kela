namespace LanguageLMS.App.Infrastructure.Http;

public sealed record ApiResult<T>(bool IsSuccess, T? Value, string? Error)
{
    public static ApiResult<T> Success(T value) => new(true, value, null);

    public static ApiResult<T> Failure(string error) => new(false, default, error);
}
