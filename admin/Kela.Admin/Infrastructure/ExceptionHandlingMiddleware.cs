using System.Net;
using Kela.Core.Common.Exceptions;

namespace Kela.Admin.Infrastructure;

/// <summary>
/// MVC action'larından sızan domain xətalarını (fetch cavabları üçün) JSON-a çevirir.
/// Səhifə naviqasiyası (Accept: text/html) üçün xəta yenidən atılır ki, xəta səhifəsi göstərilsin.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (ForbiddenException ex)
        {
            await WriteAsync(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await WriteAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error");
            if (WantsHtml(context)) throw;
            await WriteAsync(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
        }
    }

    private static bool WantsHtml(HttpContext context)
        => HttpMethods.IsGet(context.Request.Method)
           && context.Request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase);

    private static async Task WriteAsync(HttpContext context, HttpStatusCode status, string message)
    {
        if (context.Response.HasStarted) return;
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            statusCode = (int)status,
            message,
            error = message
        });
    }
}
