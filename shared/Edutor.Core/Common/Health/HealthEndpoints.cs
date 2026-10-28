using Edutor.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Core.Common.Health;

public static class HealthEndpoints
{
    /// <summary>Load balancer / uptime monitorinqi üçün anonim sağlamlıq ucu.</summary>
    public static IEndpointRouteBuilder MapEdutorHealth(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", async (HttpContext context, CancellationToken ct) =>
        {
            var db = context.RequestServices.GetRequiredService<AppDbContext>();
            try
            {
                if (await db.Database.CanConnectAsync(ct))
                    return Results.Json(new { status = "ok", database = "ok" });
            }
            catch
            {
                // Baza əlçatmazsa aşağıdakı 503 cavabı verilir.
            }

            return Results.Json(new { status = "degraded", database = "unreachable" }, statusCode: StatusCodes.Status503ServiceUnavailable);
        }).AllowAnonymous();

        return app;
    }
}
