using System.Net;
using Edutor.Api.Common.Responses;
using Edutor.Core.Common.Exceptions;
using Edutor.Core.Common.Localization;
using Microsoft.Extensions.Localization;

namespace Edutor.Api.Common.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IStringLocalizer<SharedResource> L,
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
            await WriteAsync(context, HttpStatusCode.Forbidden, ex.Message, new
            {
                code = "limit_exceeded",
                kind = ex.Kind,
                used = ex.Used,
                limit = ex.Limit
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

            // İnkişaf zamanı əsl səbəb görünsün; istehsalda detallar sızmasın.
            var message = env.IsDevelopment() ? ex.Message : L["Api_UnexpectedError"];
            await WriteAsync(
                context,
                HttpStatusCode.InternalServerError,
                message,
                env.IsDevelopment() ? new { detail = ex.ToString() } : null);
        }
    }

    private static async Task WriteAsync(
        HttpContext context, HttpStatusCode status, string message, object? data = null)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ApiResponse.Error(message, (int)status, data));
    }
}
