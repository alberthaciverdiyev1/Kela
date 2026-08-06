using System.Text.Json.Serialization;

namespace Kela.Api.Contracts;

/// <summary>
/// Tüm API yanıtları için ortak zarf (envelope).
/// Her endpoint aynı JSON şeklini döner: { success, message, data, errors }.
/// Tek mesaj <see cref="Message"/>'a, birden fazla hata/uyarı <see cref="Errors"/> listesine konur.
/// Her ikisi de hem başarı hem hata durumunda kullanılabilir.
/// </summary>
/// <typeparam name="T">Yanıt verisinin tipi (genellikle Application'daki *Response).</typeparam>
public sealed record ApiResponse<T>(
    [property: JsonPropertyName("success")]
    bool IsSuccess,
    [property: JsonPropertyName("message")]
    string? Message,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("errors")] IReadOnlyCollection<string>? Errors = null)
{
    public static ApiResponse<T> Success(T data) => new(true, null, data);

    public static ApiResponse<T> Success(T data, string? message) => new(true, message, data);

    public static ApiResponse<T> Success(T data, string? message, IReadOnlyCollection<string>? errors)
        => new(true, message, data, errors);

    public static ApiResponse<T> Error(string? message) => new(false, message, default);

    public static ApiResponse<T> Error(string? message, IReadOnlyCollection<string>? errors)
        => new(false, message, default, errors);
}
