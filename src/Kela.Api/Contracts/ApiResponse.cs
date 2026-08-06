using System.Text.Json.Serialization;

namespace Kela.Api.Contracts;

public sealed record ApiResponse(
    [property: JsonPropertyName("success")]
    bool IsSuccess,
    [property: JsonPropertyName("message")]
    string? Message,
    [property: JsonPropertyName("errors")]
    IReadOnlyCollection<string>? Errors = null)
{
    public static ApiResponse Success(string? message) => new(true, message);

    public static ApiResponse Success(string? message, IReadOnlyCollection<string>? errors)
        => new(true, message, errors);

    public static ApiResponse Error(string? message) => new(false, message);

    public static ApiResponse Error(string? message, IReadOnlyCollection<string>? errors)
        => new(false, message, errors);
}

public sealed record ApiResponse<T>(
    [property: JsonPropertyName("success")]
    bool IsSuccess,
    [property: JsonPropertyName("message")]
    string? Message,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("errors")]
    IReadOnlyCollection<string>? Errors = null)
{
    public static ApiResponse<T> Success(T data) => new(true, null, data);

    public static ApiResponse<T> Success(T data, string? message) => new(true, message, data);

    public static ApiResponse<T> Success(T data, string? message, IReadOnlyCollection<string>? errors)
        => new(true, message, data, errors);

    public static ApiResponse<T> Error(string? message) => new(false, message, default);

    public static ApiResponse<T> Error(string? message, IReadOnlyCollection<string>? errors)
        => new(false, message, default, errors);
}
