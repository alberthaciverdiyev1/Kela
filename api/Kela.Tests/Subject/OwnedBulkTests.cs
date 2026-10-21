using Kela.Core.Common.Bulk;
using Kela.Core.Common.Exceptions;
using Kela.Core.Common.Responses;
using Kela.Core.Modules.Lesson.Abstractions;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.Lesson.Responses;
using Kela.Core.Modules.Subject.Abstractions;
using Kela.Core.Modules.Subject.Requests;
using Kela.Core.Modules.Subject.Responses;
using Kela.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Tests.Subject;

[Collection("api")]
public class OwnedBulkTests(ApiFactory factory)
{
    private const long OwnerId = 9201;
    private const long OtherTeacherId = 9202;

    private async Task<T> With<TService, T>(Func<TService, Task<T>> action) where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private Task<bool> BulkAsync(BulkRequest request, long ownerId)
        => With<ISubjectService, bool>(async s =>
        {
            await s.BulkAsync(request, ownerId);
            return true;
        });

    private Task<PagedResponse<SubjectResponse>> ListAsync(long ownerId, int isDeleted)
        => With<ISubjectService, PagedResponse<SubjectResponse>>(s =>
            s.ListAsync(new SubjectListRequest { PerPage = 500, IsDeleted = isDeleted, UserId = ownerId }));

    [Fact]
    public async Task Bulk_actions_are_limited_to_the_owner()
    {
        var subject = await With<ISubjectService, SubjectResponse>(s =>
            s.CreateAsync(new SubjectUpsertRequest { Name = $"Bulk-{Guid.NewGuid():N}", UserId = OwnerId }));

        var delete = new BulkRequest { Ids = [subject.Id], Action = BulkActions.Delete };

        await Assert.ThrowsAsync<ForbiddenException>(() => BulkAsync(delete, OtherTeacherId));
        await BulkAsync(delete, OwnerId);

        Assert.Contains((await ListAsync(OwnerId, isDeleted: 1)).Data, x => x.Id == subject.Id);

        await BulkAsync(new BulkRequest { Ids = [subject.Id], Action = BulkActions.Restore }, OwnerId);

        Assert.Contains((await ListAsync(OwnerId, isDeleted: 0)).Data, x => x.Id == subject.Id);
    }

    [Fact]
    public async Task Status_can_only_be_changed_by_the_owner()
    {
        var subject = await With<ISubjectService, SubjectResponse>(s =>
            s.CreateAsync(new SubjectUpsertRequest { Name = $"Status-{Guid.NewGuid():N}", UserId = OwnerId }));

        var lesson = await With<ILessonService, LessonResponse>(l =>
            l.CreateAsync(new LessonUpsertRequest
            {
                SubjectId = subject.Id,
                Title = $"L-{Guid.NewGuid():N}",
                IsActive = true,
                UserId = OwnerId
            }));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            With<ILessonService, bool>(async l =>
            {
                await l.SetActiveAsync(lesson.Id, false, OtherTeacherId);
                return true;
            }));

        await With<ILessonService, bool>(async l =>
        {
            await l.SetActiveAsync(lesson.Id, false, OwnerId);
            return true;
        });

        var isActive = await With<AppDbContext, bool?>(async db =>
        {
            var lessonRow = await db.Lessons.IgnoreQueryFilters(new[] { "Active" })
                .FirstOrDefaultAsync(x => x.Id == lesson.Id);
            return lessonRow?.IsActive;
        });

        Assert.False(isActive);
    }
}
