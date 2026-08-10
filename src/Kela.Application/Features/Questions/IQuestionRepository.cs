using Kela.Domain.Entities;

namespace Kela.Application.Features.Questions;

public interface IQuestionRepository
{
    Task<List<Question>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<Question?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Question>> GetManyAsync(IReadOnlyList<int> ids, CancellationToken cancellationToken = default);
    void Add(Question question);
    void Update(Question question);
    void Remove(Question question);
}
