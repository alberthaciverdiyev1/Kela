using FluentValidation;
using Kela.Application.Features.Questions;
using Kela.Application.Features.Quizzes.Requests;
using Kela.Application.Features.Quizzes.Responses;
using Kela.Application.Patterns;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Quizzes;

internal sealed class QuizService(
    IQuizRepository quizzes,
    IQuestionRepository questions,
    IUnitOfWork unitOfWork,
    IValidator<AddQuizQuestionsRequest> addValidator) : IQuizService
{
    public async Task<List<QuizResponse>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        var items = await quizzes.GetByTeacherAsync(teacherId, cancellationToken);
        return items.Select(q => q.ToResponse()).ToList();
    }

    public async Task<QuizResponse?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetByContentIdWithQuestionsAsync(contentId, cancellationToken);
        return quiz?.ToResponse();
    }

    public async Task AddQuestionsAsync(int contentId, AddQuizQuestionsRequest request, CancellationToken cancellationToken = default)
    {
        await addValidator.ValidateAndThrowAsync(request, cancellationToken);

        var quiz = await quizzes.GetByContentIdWithQuestionsAsync(contentId, cancellationToken)
            ?? throw new KeyNotFoundException($"ContentId = {contentId} olan quiz bulunamadı.");

        var existing = quiz.Questions.Select(q => q.QuestionId).ToHashSet();
        var wanted = request.QuestionIds.Distinct().Where(id => !existing.Contains(id)).ToList();
        if (wanted.Count == 0)
        {
            return;
        }

        var owned = await questions.GetManyAsync(wanted, cancellationToken);
        var validIds = owned.Where(q => q.TeacherId == quiz.TeacherId).Select(q => q.Id).ToHashSet();
        var toAdd = wanted.Where(validIds.Contains).ToList();
        if (toAdd.Count == 0)
        {
            return;
        }

        var maxPos = quiz.Questions.Count == 0 ? 0 : quiz.Questions.Max(q => q.Position);
        var pos = maxPos;
        foreach (var qid in toAdd)
        {
            pos++;
            quiz.Questions.Add(new QuizQuestion { QuizId = contentId, QuestionId = qid, Position = pos });
        }

        quiz.UpdatedAt = DateTime.UtcNow;
        quizzes.Update(quiz);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveQuestionAsync(int contentId, int questionId, CancellationToken cancellationToken = default)
    {
        var quiz = await quizzes.GetByContentIdWithQuestionsAsync(contentId, cancellationToken)
            ?? throw new KeyNotFoundException($"ContentId = {contentId} olan quiz bulunamadı.");

        var item = quiz.Questions.FirstOrDefault(q => q.QuestionId == questionId);
        if (item is null)
        {
            return;
        }

        quiz.Questions.Remove(item);

        var ordered = quiz.Questions.OrderBy(q => q.Position).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Position = i + 1;
        }

        quiz.UpdatedAt = DateTime.UtcNow;
        quizzes.Update(quiz);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
