using Kela.Application.Features.Quizzes;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class QuizRepository(KelaDbContext context) : IQuizRepository
{
    public async Task<List<Quiz>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default)
        => await context.Quizzes
            .Where(q => q.TeacherId == teacherId)
            .Include(q => q.Questions)
            .ThenInclude(z => z.Question)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Quiz?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default)
        => await context.Quizzes.FirstOrDefaultAsync(q => q.ContentId == contentId, cancellationToken);

    public async Task<Quiz?> GetByContentIdWithQuestionsAsync(int contentId, CancellationToken cancellationToken = default)
        => await context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(z => z.Question)
            .FirstOrDefaultAsync(q => q.ContentId == contentId, cancellationToken);

    public void Add(Quiz quiz) => context.Quizzes.Add(quiz);

    public void Update(Quiz quiz) => context.Quizzes.Update(quiz);

    public void Remove(Quiz quiz) => context.Quizzes.Remove(quiz);

    public async Task<int> RemoveQuestionRefsAsync(int questionId, CancellationToken cancellationToken = default)
        => await context.QuizQuestions
            .Where(z => z.QuestionId == questionId)
            .ExecuteDeleteAsync(cancellationToken);
}
