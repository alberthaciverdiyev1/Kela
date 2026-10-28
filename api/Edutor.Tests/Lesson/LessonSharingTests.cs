using Edutor.Core.Common.Exceptions;
using Edutor.Core.Modules.Lesson.Abstractions;
using Edutor.Core.Modules.Lesson.Requests;
using Edutor.Core.Modules.Lesson.Responses;
using Edutor.Core.Modules.Subject.Abstractions;
using Edutor.Core.Modules.Subject.Requests;
using Edutor.Core.Modules.Subject.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Tests.Lesson;

/// <summary>
/// Dərs paylaşımı: sahibi paylaşır, paylaşılandan sonra digər müəllim statusu
/// dəyişə bilir; admin hər dərsi paylaşa bilir.
/// </summary>
[Collection("api")]
public class LessonSharingTests(ApiFactory factory)
{
    private const long OwnerId = 9301;
    private const long OtherTeacherId = 9302;

    private async Task<T> With<TService, T>(Func<TService, Task<T>> action) where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private async Task<(LessonResponse Lesson, long SubjectId)> CreateLessonAsync(
        long ownerId, bool isShared = false, bool subjectShared = false)
    {
        var subject = await With<ISubjectService, SubjectResponse>(s =>
            s.CreateAsync(new SubjectUpsertRequest
            {
                Name = $"Share-{Guid.NewGuid():N}",
                UserId = ownerId,
                IsShared = subjectShared
            }));

        var lesson = await With<ILessonService, LessonResponse>(l => l.CreateAsync(new LessonUpsertRequest
        {
            SubjectId = subject.Id,
            Title = $"L-{Guid.NewGuid():N}",
            IsActive = true,
            IsShared = isShared,
            UserId = ownerId
        }));

        return (lesson, subject.Id);
    }

    /// <summary>API hostunda "Active" filtri var — deaktiv sətri birbaşa bazadan oxuyuruq.</summary>
    private Task<Edutor.Core.Modules.Lesson.Entities.Lesson> RowAsync(long id)
        => With<Edutor.Core.Data.AppDbContext, Edutor.Core.Modules.Lesson.Entities.Lesson>(db =>
            db.Lessons.IgnoreQueryFilters(["SoftDelete", "Active"]).AsNoTracking()
                .FirstAsync(x => x.Id == id));

    private Task SetActiveAsync(long lessonId, bool active, long? ownerId)
        => With<ILessonService, bool>(async s =>
        {
            await s.SetActiveAsync(lessonId, active, ownerId);
            return true;
        });

    private Task<LessonResponse> UpdateAsync(long lessonId, long subjectId, long? ownerId, bool isShared, bool isActive = true)
        => With<ILessonService, LessonResponse>(l => l.UpdateAsync(lessonId, new LessonUpsertRequest
        {
            SubjectId = subjectId,
            Title = "updated",
            IsActive = isActive,
            IsShared = isShared
        }, ownerId));

    [Fact]
    public async Task Sharing_a_lesson_lets_another_teacher_change_its_status()
    {
        var (lesson, subjectId) = await CreateLessonAsync(OwnerId);

        // Paylaşılmamış dərsin statusu başqa müəllimə bağlıdır.
        await Assert.ThrowsAsync<ForbiddenException>(() => SetActiveAsync(lesson.Id, false, OtherTeacherId));

        // Sahibi paylaşır.
        await UpdateAsync(lesson.Id, subjectId, OwnerId, isShared: true);

        // Artıq digər müəllim statusu dəyişə bilir.
        await SetActiveAsync(lesson.Id, false, OtherTeacherId);

        // API hostunda "Active" filtri var — deaktiv dərsi birbaşa bazadan oxuyuruq.
        var row = await RowAsync(lesson.Id);
        Assert.False(row.IsActive);
        Assert.True(row.IsShared);
    }

    [Fact]
    public async Task A_shared_lesson_is_visible_to_other_teachers()
    {
        var (lesson, subjectId) = await CreateLessonAsync(OwnerId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            With<ILessonService, LessonResponse>(l => l.GetAsync(lesson.Id, OtherTeacherId)));

        await UpdateAsync(lesson.Id, subjectId, OwnerId, isShared: true);

        var seen = await With<ILessonService, LessonResponse>(l => l.GetAsync(lesson.Id, OtherTeacherId));
        Assert.Equal(lesson.Id, seen.Id);
    }

    [Fact]
    public async Task A_teacher_cannot_share_someone_elses_lesson()
    {
        var (lesson, subjectId) = await CreateLessonAsync(OwnerId);

        // Başqa müəllim nə paylaşımı aça, nə də dərsi redaktə edə bilər.
        await Assert.ThrowsAsync<ForbiddenException>(() => UpdateAsync(lesson.Id, subjectId, OtherTeacherId, isShared: true));

        var unchanged = await With<ILessonService, LessonResponse>(l => l.GetAsync(lesson.Id, OwnerId));
        Assert.False(unchanged.IsShared);
    }

    [Fact]
    public async Task An_admin_can_share_any_lesson()
    {
        var (lesson, subjectId) = await CreateLessonAsync(OwnerId);

        // Admin ownerId göndərmir — sahiblik yoxlaması tətbiq olunmur.
        await UpdateAsync(lesson.Id, subjectId, ownerId: null, isShared: true);

        await SetActiveAsync(lesson.Id, false, OtherTeacherId);

        var row = await RowAsync(lesson.Id);
        Assert.True(row.IsShared);
        Assert.False(row.IsActive);
    }

    [Fact]
    public async Task A_lesson_under_a_shared_subject_also_allows_status_changes()
    {
        var (lesson, subjectId) = await CreateLessonAsync(OwnerId, subjectShared: true);

        await SetActiveAsync(lesson.Id, false, OtherTeacherId);

        Assert.False((await RowAsync(lesson.Id)).IsActive);
    }

    [Fact]
    public async Task Unsharing_puts_the_status_back_in_the_owners_hands()
    {
        var (lesson, subjectId) = await CreateLessonAsync(OwnerId, isShared: true);

        await UpdateAsync(lesson.Id, subjectId, OwnerId, isShared: false);

        await Assert.ThrowsAsync<ForbiddenException>(() => SetActiveAsync(lesson.Id, false, OtherTeacherId));
    }
}
