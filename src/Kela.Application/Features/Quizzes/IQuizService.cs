using Kela.Application.Features.Quizzes.Requests;
using Kela.Application.Features.Quizzes.Responses;

namespace Kela.Application.Features.Quizzes;

public interface IQuizService
{
    Task<List<QuizResponse>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<QuizResponse?> GetByContentIdAsync(int contentId, CancellationToken cancellationToken = default);
    Task AddQuestionsAsync(int contentId, AddQuizQuestionsRequest request, CancellationToken cancellationToken = default);
    Task RemoveQuestionAsync(int contentId, int questionId, CancellationToken cancellationToken = default);
}
