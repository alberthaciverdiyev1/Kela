using System.Net;
using Kela.Core.Common.Exceptions;

namespace Kela.Admin.Infrastructure;

/// <summary>
/// MVC action'larından sızan domain xətalarını (fetch cavabları üçün) JSON-a çevirir.
/// Səhifə naviqasiyası (Accept: text/html) üçün xəta yenidən atılır ki, xəta səhifəsi göstərilsin.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IWebHostEnvironment env)
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
        catch (LimitExceededException ex)
        {
            await WriteAsync(context, HttpStatusCode.Forbidden, ex.Message, new Dictionary<string, object?>
            {
                ["code"] = "limit_exceeded",
                ["kind"] = ex.Kind,
                ["used"] = ex.Used,
                ["limit"] = ex.Limit
            });
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

            // HTML səhifə naviqasiyasında framework-ün öz detallı xəta səhifəsi göstərilsin.
            if (WantsHtml(context)) throw;

            // İnkişaf zamanı əsl səbəb görünsün; istehsalda detallar sızmasın.
            await WriteAsync(
                context,
                HttpStatusCode.InternalServerError,
                env.IsDevelopment() ? ex.Message : "An unexpected error occurred.",
                detail: env.IsDevelopment() ? ex.ToString() : null);
        }
    }

    private static bool WantsHtml(HttpContext context)
        => HttpMethods.IsGet(context.Request.Method)
           && context.Request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase);

    private static async Task WriteAsync(
        HttpContext context, HttpStatusCode status, string message,
        IDictionary<string, object?>? extra = null, string? detail = null)
    {
        if (context.Response.HasStarted) return;

        var body = new Dictionary<string, object?>
        {
            ["success"] = false,
            ["statusCode"] = (int)status,
            ["message"] = message,
            ["error"] = message
        };

        if (extra is not null)
        {
            foreach (var item in extra) body[item.Key] = item.Value;
        }

        if (detail is not null) body["detail"] = detail;

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(body);
    }
}
