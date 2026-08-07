namespace Kela.Web.Infrastructure;

public sealed record ApiEnvelope<T>(
    int StatusCode,
    bool Success,
    string? Message,
    T? Data,
    IReadOnlyCollection<string>? Errors = null);

public sealed record ApiResult<T>(
    bool Success,
    int StatusCode,
    T? Data,
    string? Message,
    string? SetCookie,
    IReadOnlyCollection<string>? Errors = null);

public sealed record NoContentData;

public sealed record LoginRequest(string Email, string Password);

public sealed record RegisterRequest(string FirstName, string LastName, string Email, string Password);

public sealed record LoginResponse(int UserId, string FirstName, string LastName, string Role);

public sealed record RegisterResponse(int UserId, string FirstName, string LastName, string Email);

public sealed record SiteConfigResponse(
    string SiteName,
    string PrimaryColor,
    string SecondaryColor,
    string SuccessColor,
    string WarningColor,
    string ErrorColor,
    string InfoColor,
    string NavMode);

public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);

public sealed record CreateStudentRequest(
    string FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Email,
    DateOnly? BirthDate,
    int? CityId);

public sealed record StudentResponse(
    int Id,
    int UserId,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Email,
    DateOnly? BirthDate,
    int? CityId,
    string? CityName,
    DateTime CreatedAt);

public sealed record StudentCreatedResponse(int Id, int UserId, string Email, string Password, DateTime CreatedAt);
