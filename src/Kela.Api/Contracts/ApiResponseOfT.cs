using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Kela.Api.Contracts;

/// <summary>
/// Data taşıyan yanıt zarfı: { statusCode, success, message, data, errors }.
/// HTTP status kodu zarfın içinde taşınır; HTTP sonucunu (IResult) üreten fabrika metotları
/// burada toplanır. Liste, get-by-id, oluşturma (201) ve login gibi payload dönen yanıtlar
/// bu formu kullanır. Data içermeyen yanıtlar için <see cref="ApiResponse"/> kullanılır.
/// </summary>
/// <typeparam name="T">Yanıt verisinin tipi (genellikle Application'daki *Response).</typeparam>
public sealed record ApiResponse<T>(
    [property: JsonPropertyName("statusCode")]
    int StatusCode,
    [property: JsonPropertyName("success")]
    bool IsSuccess,
    [property: JsonPropertyName("message")]
    string? Message,
    [property: JsonPropertyName("data")] T? Data,
    [property: JsonPropertyName("errors")] IReadOnlyCollection<string>? Errors = null)
{
    // 200 — data'lı başarı
    public static IResult Success(T data) => Results.Ok(new ApiResponse<T>(StatusCodes.Status200OK, true, null, data));

    public static IResult Success(T data, string message) =>
        Results.Ok(new ApiResponse<T>(StatusCodes.Status200OK, true, message, data));

    public static IResult Success(T data, string message, IReadOnlyCollection<string>? errors)
        => Results.Ok(new ApiResponse<T>(StatusCodes.Status200OK, true, message, data, errors));

    // 201 — kaynak oluşturuldu (Location header + data)
    public static IResult Created(string location, T data)
        => Results.Created(location, new ApiResponse<T>(StatusCodes.Status201Created, true, null, data));

    public static IResult Created(string location, T data, string message)
        => Results.Created(location, new ApiResponse<T>(StatusCodes.Status201Created, true, message, data));

    // 4xx — data'sız hata (mesaj-only forma yönlendirir; T'li bağlamda kolaylık)
    public static IResult BadRequest(string message) => ApiResponse.BadRequest(message);

    public static IResult Unauthorized(string message) => ApiResponse.Unauthorized(message);

    public static IResult Forbidden(string message) => ApiResponse.Forbidden(message);

    public static IResult NotFound(string message) => ApiResponse.NotFound(message);

    public static IResult Conflict(string message) => ApiResponse.Conflict(message);

    public static IResult UnprocessableEntity(string message) => ApiResponse.UnprocessableEntity(message);
}
