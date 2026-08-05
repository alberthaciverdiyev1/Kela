using Kela.Domain.Grades;

namespace Kela.Application.Repositories;

public interface IGradeRepository
{
    Task<Grade?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Grade>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);

    void Add(Grade grade);
    void Update(Grade grade);
    void Remove(Grade grade);
}
