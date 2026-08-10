using Kela.Domain.Entities;

namespace Kela.Application.Features.Quizzes;

public interface IQuizRepository
{
    Task<List<Quiz>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<Quiz?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default);
    Task<Quiz?> GetByContentIdWithQuestionsAsync(int contentId, CancellationToken cancellationToken = default);
    void Add(Quiz quiz);
    void Update(Quiz quiz);
    void Remove(Quiz quiz);
    Task<int> RemoveQuestionRefsAsync(int questionId, CancellationToken cancellationToken = default);
}
