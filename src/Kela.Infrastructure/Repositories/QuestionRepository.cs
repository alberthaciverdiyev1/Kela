using Kela.Application.Features.Questions;
using Kela.Domain.Entities;
using Kela.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kela.Infrastructure.Repositories;

internal sealed class QuestionRepository(KelaDbContext context) : IQuestionRepository
{
    public async Task<List<Question>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default)
        => await context.Questions
            .Where(q => q.TeacherId == teacherId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<Question?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await context.Questions.FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public async Task<List<Question>> GetManyAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default)
        => await context.Questions
            .Where(q => ids.Contains(q.Id))
            .ToListAsync(cancellationToken);

    public void Add(Question question) => context.Questions.Add(question);

    public void Update(Question question) => context.Questions.Update(question);

    public void Remove(Question question) => context.Questions.Remove(question);
}
