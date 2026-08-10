using FluentValidation;
using Kela.Application.Features.Questions.Requests;
using Kela.Application.Features.Questions.Responses;
using Kela.Application.Features.Quizzes;
using Kela.Application.Patterns;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Questions;

internal sealed class QuestionService(
    IQuestionRepository questions,
    IQuizRepository quizzes,
    IUnitOfWork unitOfWork,
    IValidator<CreateQuestionRequest> createValidator,
    IValidator<UpdateQuestionRequest> updateValidator) : IQuestionService
{
    public async Task<List<QuestionResponse>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default)
    {
        var items = await questions.GetByTeacherAsync(teacherId, cancellationToken);
        return items.Select(q => q.ToResponse()).ToList();
    }

    public async Task<QuestionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await questions.GetByIdAsync(id, cancellationToken);
        return question?.ToResponse();
    }

    public async Task<int> CreateAsync(CreateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var question = new Question
        {
            TeacherId = request.TeacherId,
            Text = request.Text.Trim(),
            OptionA = request.OptionA.Trim(),
            OptionB = request.OptionB.Trim(),
            OptionC = request.OptionC.Trim(),
            OptionD = TrimNull(request.OptionD),
            OptionE = TrimNull(request.OptionE),
            CorrectOption = request.CorrectOption,
            CreatedAt = DateTime.UtcNow,
        };

        questions.Add(question);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return question.Id;
    }

    public async Task UpdateAsync(int id, UpdateQuestionRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var question = await questions.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan soru bulunamadı.");

        question.Text = request.Text.Trim();
        question.OptionA = request.OptionA.Trim();
        question.OptionB = request.OptionB.Trim();
        question.OptionC = request.OptionC.Trim();
        question.OptionD = TrimNull(request.OptionD);
        question.OptionE = TrimNull(request.OptionE);
        question.CorrectOption = request.CorrectOption;
        question.UpdatedAt = DateTime.UtcNow;

        questions.Update(question);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var question = await questions.GetByIdAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {id} olan soru bulunamadı.");

        question.DeletedAt = DateTime.UtcNow;
        questions.Update(question);

        await quizzes.RemoveQuestionRefsAsync(id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string? TrimNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return value.Trim();
    }
}
