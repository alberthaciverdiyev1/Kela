using Kela.Application.Features.Questions.Requests;
using Kela.Application.Features.Questions.Responses;

namespace Kela.Application.Features.Questions;

public interface IQuestionService
{
    Task<List<QuestionResponse>> GetByTeacherAsync(int teacherId, CancellationToken cancellationToken = default);
    Task<QuestionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(CreateQuestionRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateQuestionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
