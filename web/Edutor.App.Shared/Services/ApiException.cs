namespace Edutor.App.Shared.Services;

public sealed class ApiException(string message, int? statusCode = null) : Exception(message)
{
    public int? StatusCode { get; } = statusCode;
}
