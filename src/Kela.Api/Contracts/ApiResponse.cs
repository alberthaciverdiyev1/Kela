using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Kela.Api.Contracts;

/// <summary>
/// Data içermeyen yanıt zarfı (mesaj-only): { statusCode, success, message, errors }.
/// HTTP status kodu zarfın içinde taşınır; HTTP sonucunu (IResult) üreten fabrika metotları
/// burada toplanır. 401/404, GlobalExceptionHandler ve "veri dönmeyen" yanıtlar bu formu kullanır.
/// Data dönen yanıtlar için <see cref="ApiResponse{T}"/> kullanılır.
/// </summary>
public sealed record ApiResponse(
    [property: JsonPropertyName("statusCode")]
    int StatusCode,
    [property: JsonPropertyName("success")]
    bool IsSuccess,
    [property: JsonPropertyName("message")]
    string? Message,
    [property: JsonPropertyName("errors")] IReadOnlyCollection<string>? Errors = null)
{
    // 200 — mesaj-only başarı
    public static IResult Success(string message) =>
        Results.Ok(new ApiResponse(StatusCodes.Status200OK, true, message));

    public static IResult Success(string message, IReadOnlyCollection<string>? errors)
        => Results.Ok(new ApiResponse(StatusCodes.Status200OK, true, message, errors));

    // 204 — gövdesiz başarı
    public static IResult NoContent() => Results.NoContent();

    // 400 — geçersiz istek / doğrulama hatası
    public static IResult BadRequest(string message) => Error(StatusCodes.Status400BadRequest, message);

    public static IResult BadRequest(string message, IReadOnlyCollection<string>? errors)
        => Error(StatusCodes.Status400BadRequest, message, errors);

    // 401 — kimlik doğrulanamadı
    public static IResult Unauthorized(string message) => Error(StatusCodes.Status401Unauthorized, message);

    // 403 — yetki yok (kimlik var ama rol/izin yetmiyor)
    public static IResult Forbidden(string message) => Error(StatusCodes.Status403Forbidden, message);

    // 404 — kaynak bulunamadı
    public static IResult NotFound(string message) => Error(StatusCodes.Status404NotFound, message);

    // 409 — durum çakışması (ör. eşsiz alan zaten kayıtlı)
    public static IResult Conflict(string message) => Error(StatusCodes.Status409Conflict, message);

    // 422 — işlenemeyen içerik (yapı doğru ama semantik olarak geçersiz)
    public static IResult UnprocessableEntity(string message) => Error(StatusCodes.Status422UnprocessableEntity, message);

    public static IResult UnprocessableEntity(string message, IReadOnlyCollection<string>? errors)
        => Error(StatusCodes.Status422UnprocessableEntity, message, errors);

    // Jenerik — status kodunu çağıran belirler (GlobalExceptionHandler gibi)
    public static IResult Error(int statusCode, string message) => Error(statusCode, message, null);

    public static IResult Error(int statusCode, string message, IReadOnlyCollection<string>? errors)
        => Results.Json(new ApiResponse(statusCode, false, message, errors), statusCode: statusCode);
}
