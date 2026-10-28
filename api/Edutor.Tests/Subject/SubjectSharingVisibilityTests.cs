using Edutor.Core.Common.Exceptions;
using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.Lesson.Abstractions;
using Edutor.Core.Modules.Lesson.Requests;
using Edutor.Core.Modules.Lesson.Responses;
using Edutor.Core.Modules.LessonContent.Abstractions;
using Edutor.Core.Modules.LessonContent.Requests;
using Edutor.Core.Modules.LessonContent.Responses;
using Edutor.Core.Modules.Subject.Abstractions;
using Edutor.Core.Modules.Subject.Requests;
using Edutor.Core.Modules.Subject.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace Edutor.Tests.Subject;

[Collection("api")]
public class SubjectSharingVisibilityTests(ApiFactory factory)
{
    private const long OwnerId = 9101;
    private const long OtherTeacherId = 9102;

    private static string Unique(string p) => $"{p}-{Guid.NewGuid():N}";

    private async Task<T> With<TService, T>(Func<TService, Task<T>> action) where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }

    private async Task<(long SubjectId, long LessonId, long ContentId)> CreateCourseAsync(string prefix, bool shared)
    {
        var subject = await With<ISubjectService, SubjectResponse>(s =>
            s.CreateAsync(new SubjectUpsertRequest { Name = Unique(prefix), UserId = OwnerId, IsShared = shared }));

        var lesson = await With<ILessonService, LessonResponse>(l =>
            l.CreateAsync(new LessonUpsertRequest { SubjectId = subject.Id, Title = Unique("L"), IsActive = true, UserId = OwnerId }));

        var content = await With<ILessonContentService, LessonContentResponse>(c =>
            c.CreateAsync(new LessonContentCreateRequest
            {
                LessonId = lesson.Id,
                UserId = OwnerId,
                Title = Unique("C"),
                IsActive = true
            }));

        return (subject.Id, lesson.Id, content.Id);
    }

    private Task<PagedResponse<SubjectResponse>> SubjectsAsync(long? userId)
        => With<ISubjectService, PagedResponse<SubjectResponse>>(s =>
            s.ListAsync(new SubjectListRequest { PerPage = 500, UserId = userId }));

    private Task<PagedResponse<LessonResponse>> LessonsAsync(long? userId)
        => With<ILessonService, PagedResponse<LessonResponse>>(l =>
            l.ListAsync(new LessonListRequest { PerPage = 500, UserId = userId }));

    private Task<PagedResponse<LessonContentResponse>> ContentsAsync(long? userId)
        => With<ILessonContentService, PagedResponse<LessonContentResponse>>(c =>
            c.ListAsync(new LessonContentListRequest { PerPage = 500, UserId = userId }));

    [Fact]
    public async Task Private_subject_is_visible_only_to_its_owner()
    {
        var (subjectId, lessonId, contentId) = await CreateCourseAsync("Private", shared: false);

        Assert.Contains((await SubjectsAsync(OwnerId)).Data, x => x.Id == subjectId);
        Assert.DoesNotContain((await SubjectsAsync(OtherTeacherId)).Data, x => x.Id == subjectId);
        Assert.DoesNotContain((await LessonsAsync(OtherTeacherId)).Data, x => x.Id == lessonId);
        Assert.DoesNotContain((await ContentsAsync(OtherTeacherId)).Data, x => x.Id == contentId);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            With<ISubjectService, SubjectResponse>(s => s.GetAsync(subjectId, OtherTeacherId)));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            With<ILessonService, LessonResponse>(l => l.GetAsync(lessonId, OtherTeacherId)));
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            With<ILessonContentService, LessonContentResponse>(c => c.GetAsync(contentId, OtherTeacherId)));
    }

    [Fact]
    public async Task Shared_subject_exposes_its_lessons_and_contents_to_every_teacher()
    {
        var (subjectId, lessonId, contentId) = await CreateCourseAsync("Shared", shared: true);

        Assert.Contains((await SubjectsAsync(OtherTeacherId)).Data, x => x.Id == subjectId);
        Assert.Contains((await LessonsAsync(OtherTeacherId)).Data, x => x.Id == lessonId);
        Assert.Contains((await ContentsAsync(OtherTeacherId)).Data, x => x.Id == contentId);

        var subject = await With<ISubjectService, SubjectResponse>(s => s.GetAsync(subjectId, OtherTeacherId));
        var lesson = await With<ILessonService, LessonResponse>(l => l.GetAsync(lessonId, OtherTeacherId));
        var content = await With<ILessonContentService, LessonContentResponse>(c => c.GetAsync(contentId, OtherTeacherId));

        Assert.Equal(subjectId, subject.Id);
        Assert.Equal(lessonId, lesson.Id);
        Assert.Equal(contentId, content.Id);
    }

    [Fact]
    public async Task Admins_still_see_every_subject()
    {
        var (subjectId, _, _) = await CreateCourseAsync("AdminScope", shared: false);

        Assert.Contains((await SubjectsAsync(null)).Data, x => x.Id == subjectId);
    }
}
