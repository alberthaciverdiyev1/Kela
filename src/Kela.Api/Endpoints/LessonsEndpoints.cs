using System.Security.Claims;
using Kela.Api.Contracts;
using Kela.Api.Media;
using Kela.Application.Features.Lessons;
using Kela.Application.Features.Lessons.Responses;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class LessonsEndpoints
{
    public static IEndpointRouteBuilder MapLessonsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/lessons")
            .RequireAuthorization();

        // Ders detayı (editor: sahip öğretmen/admin; öğrenci: yayınlanmış ders).
        group.MapGet("/{contentId:int}", async (
            int contentId,
            ClaimsPrincipal principal,
            ILessonService lessons,
            CancellationToken ct) =>
        {
            var userId = GetUserId(principal);
            var lesson = await lessons.GetByContentIdAsync(contentId, ct);
            if (lesson is null)
            {
                return ApiResponse.NotFound("Ders bulunamadı.");
            }

            if (!CanView(principal, userId, lesson))
            {
                return ApiResponse.Forbidden("Bu derse erişim yetkiniz yok.");
            }

            return ApiResponse<LessonResponse>.Success(lesson);
        });

        // Video yükleme: diske kaydeder, süreyi ffprobe ile okur, thumbnail'i ffmpeg ile çıkarır.
        // Web katmanı CSRF'yi [ValidateAntiForgeryToken] ile doğruladığı için burada antiforgery devre dışı.
        group.MapPost("/{contentId:int}/video", async (
            int contentId,
            IFormFile file,
            ClaimsPrincipal principal,
            ILessonService lessons,
            MediaProcessor media,
            CancellationToken ct) =>
        {
            var userId = GetUserId(principal);
            var lesson = await lessons.GetByContentIdAsync(contentId, ct);
            if (lesson is null)
            {
                return ApiResponse.NotFound("Ders bulunamadı.");
            }

            if (!IsTeacher(principal) && !IsAdmin(principal))
            {
                return ApiResponse.Forbidden("Yalnızca öğretmenler video yükleyebilir.");
            }

            if (!IsAdmin(principal) && lesson.TeacherId != userId)
            {
                return ApiResponse.Forbidden("Bu ders size ait değil.");
            }

            if (file is null || file.Length == 0)
            {
                return ApiResponse.BadRequest("Video dosyası gerekli.");
            }

            if (file.Length > 512L * 1024 * 1024)
            {
                return ApiResponse.BadRequest("Video 512 MB'dan büyük olamaz.");
            }

            await using var stream = file.OpenReadStream();
            var (videoPath, thumbPath, duration) = media.SaveVideo(stream, file.FileName);

            await lessons.SetVideoAsync(contentId, videoPath, thumbPath, duration, ct);

            var updated = await lessons.GetByContentIdAsync(contentId, ct);
            return updated is null
                ? ApiResponse.NotFound("Ders bulunamadı.")
                : ApiResponse<LessonResponse>.Success(updated);
        }).DisableAntiforgery();

        // Ders sırasını güncelle.
        group.MapPut("/{contentId:int}/order", async (
            int contentId,
            int orderIndex,
            ClaimsPrincipal principal,
            ILessonService lessons,
            CancellationToken ct) =>
        {
            var userId = GetUserId(principal);
            var lesson = await lessons.GetByContentIdAsync(contentId, ct);
            if (lesson is null)
            {
                return ApiResponse.NotFound("Ders bulunamadı.");
            }

            if (!IsTeacher(principal) && !IsAdmin(principal))
            {
                return ApiResponse.Forbidden("Yalnızca öğretmenler ders sırasını değiştirebilir.");
            }

            if (!IsAdmin(principal) && lesson.TeacherId != userId)
            {
                return ApiResponse.Forbidden("Bu ders size ait değil.");
            }

            await lessons.UpdateOrderAsync(contentId, orderIndex, ct);
            return ApiResponse.NoContent();
        });

        // Video akışı (Range destekli, öğrenciler yayınlanmış dersleri izleyebilir).
        group.MapGet("/{contentId:int}/stream", async (
            int contentId,
            ClaimsPrincipal principal,
            ILessonService lessons,
            MediaProcessor media,
            CancellationToken ct) =>
        {
            var userId = GetUserId(principal);
            var lesson = await lessons.GetByContentIdAsync(contentId, ct);
            if (lesson is null)
            {
                return Results.NotFound();
            }

            if (!CanView(principal, userId, lesson))
            {
                return Results.StatusCode(StatusCodes.Status403Forbidden);
            }

            var physical = media.ResolvePhysicalPath(lesson.VideoPath ?? "");
            if (physical is null)
            {
                return Results.NotFound();
            }

            return Results.File(
                physical,
                MediaProcessor.ContentTypeFor(physical),
                enableRangeProcessing: true);
        });

        return app;
    }

    private static int GetUserId(ClaimsPrincipal principal)
        => int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    private static bool IsTeacher(ClaimsPrincipal principal) => principal.IsInRole(RoleNames.Teacher);
    private static bool IsAdmin(ClaimsPrincipal principal) => principal.IsInRole(RoleNames.Admin);

    private static bool CanView(ClaimsPrincipal principal, int userId, LessonResponse lesson)
    {
        if (IsAdmin(principal))
        {
            return true;
        }

        if (IsTeacher(principal))
        {
            return lesson.TeacherId == userId;
        }

        // Öğrenci (ve diğer rolleri) yalnızca yayınlanmış dersleri izleyebilir.
        return lesson.IsPublished;
    }
}
