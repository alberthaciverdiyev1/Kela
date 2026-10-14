using System.Text.Json.Serialization;

namespace Kela.Api.Common.Responses;

public sealed record ApiResponse<T>(
    bool Success,
    int StatusCode,
    string Message,
    T? Data,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IDictionary<string, string[]>? Errors = null);

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string message = "", int statusCode = 200)
        => new(true, statusCode, message, data);

    public static ApiResponse<string> Success(string message, int statusCode = 200)
        => new(true, statusCode, message, message);

    public static ApiResponse<object?> Error(
        string message,
        int statusCode = 400,
        object? data = null,
        IDictionary<string, string[]>? errors = null)
        => new(false, statusCode, message, data, errors);
}
