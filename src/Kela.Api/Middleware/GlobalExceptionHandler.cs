using Kela.Api.Contracts;
using Kela.Application.Validation;
using Microsoft.AspNetCore.Diagnostics;

namespace Kela.Api.Middleware;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationException validation => (
                StatusCodes.Status400BadRequest,
                "Doğrulama hatası.",
                (IReadOnlyCollection<string>?)validation.Errors),
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message, null),
            InvalidOperationException => (StatusCodes.Status400BadRequest, exception.Message, null),
            _ => (StatusCodes.Status500InternalServerError, "Beklenmeyen bir hata oluştu.", null),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse<object>.Error(message, errors), cancellationToken);

        return true;
    }
}
