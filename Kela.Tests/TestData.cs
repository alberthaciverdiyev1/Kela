using Kela.Core.Modules.Group.Abstractions;
using Kela.Core.Modules.Group.Requests;
using Kela.Core.Modules.Lesson.Abstractions;
using Kela.Core.Modules.Lesson.Requests;
using Kela.Core.Modules.LessonContent.Abstractions;
using Kela.Core.Modules.LessonContent.Entities;
using Kela.Core.Modules.LessonContent.Requests;
using Kela.Core.Modules.Question.Abstractions;
using Kela.Core.Modules.Question.Requests;
using Kela.Core.Modules.Quiz.Abstractions;
using Kela.Core.Modules.Quiz.Requests;
using Kela.Core.Modules.Subject.Abstractions;
using Kela.Core.Modules.Subject.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Tests;

/// <summary>
/// Test datasını HTTP (teacher ucları) yerinə birbaşa Core servisləri ilə yaradır.
/// API yalnız şagird üçündür, ona görə aranjiman servislərlə edilir.
/// </summary>
public static class TestData
{
    private static async Task<TResult> UseAsync<TService, TResult>(ApiFactory factory, Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return await action(service);
    }

    private static async Task UseAsync<TService>(ApiFactory factory, Func<TService, Task> action)
        where TService : notnull
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        await action(service);
    }

    public static Task<long> SubjectAsync(ApiFactory factory, string name)
        => UseAsync<ISubjectService, long>(factory,
            async s => (await s.CreateAsync(new SubjectUpsertRequest { Name = name })).Id);

    public static Task<long> LessonAsync(ApiFactory factory, long subjectId, string title)
        => UseAsync<ILessonService, long>(factory,
            async s => (await s.CreateAsync(new LessonUpsertRequest { SubjectId = subjectId, Title = title, IsActive = true })).Id);

    public static Task<long> GroupAsync(ApiFactory factory, string name)
        => UseAsync<IGroupService, long>(factory,
            async s => (await s.CreateAsync(new GroupUpsertRequest { Name = name })).Id);

    public static Task AssignSubjectsAsync(ApiFactory factory, long groupId, params long[] subjectIds)
        => UseAsync<IGroupService>(factory,
            s => s.AssignSubjectsAsync(groupId, new AssignSubjectsRequest { SubjectIds = subjectIds.ToList() }));

    public static Task AssignStudentsAsync(ApiFactory factory, long groupId, IEnumerable<long> studentIds,
        DateOnly? start = null, DateOnly? end = null)
        => UseAsync<IGroupService>(factory,
            s => s.AssignStudentsAsync(groupId, new AssignStudentsRequest
            {
                StudentIds = studentIds.ToList(),
                StartDate = start,
                EndDate = end
            }));

    public static Task<long> LessonContentAsync(ApiFactory factory, long lessonId, string title, LessonContentType type = LessonContentType.Video)
        => UseAsync<ILessonContentService, long>(factory,
            async s => (await s.CreateAsync(new LessonContentCreateRequest
            {
                LessonId = lessonId,
                Title = title,
                Type = type,
                IsActive = true,
                Path = $"test/{Guid.NewGuid():N}.mp4"
            })).Id);

    public static Task<long> QuestionAsync(ApiFactory factory, long subjectId, string title, string correct = "A")
        => UseAsync<IQuestionService, long>(factory,
            async s => (await s.CreateAsync(new QuestionUpsertRequest
            {
                SubjectId = subjectId,
                Title = title,
                VariantA = "a",
                VariantB = "b",
                VariantC = "c",
                VariantD = "d",
                CorrectAnswer = correct,
                TitleType = "text",
                VariantType = "text"
            })).Id);

    public static Task<long> QuizAsync(ApiFactory factory, long lessonId, string title)
        => UseAsync<IQuizService, long>(factory,
            async s => (await s.CreateAsync(new QuizUpsertRequest
            {
                LessonId = lessonId,
                Title = title,
                Type = "lesson",
                IsActive = true
            })).Id);

    public static Task AssignQuestionsAsync(ApiFactory factory, long quizId, params long[] questionIds)
        => UseAsync<IQuizService>(factory,
            s => s.AssignQuestionsAsync(quizId, new AssignQuestionsRequest { QuestionIds = questionIds.ToList() }));
}
