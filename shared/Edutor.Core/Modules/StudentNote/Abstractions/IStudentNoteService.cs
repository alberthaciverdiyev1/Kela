using Edutor.Core.Modules.StudentNote.Requests;
using Edutor.Core.Modules.StudentNote.Responses;

namespace Edutor.Core.Modules.StudentNote.Abstractions;

public interface IStudentNoteService
{
    Task<IReadOnlyList<StudentNoteResponse>> ListAsync(
        long studentId, StudentNoteListRequest request, CancellationToken ct = default);

    Task<StudentNoteResponse> CreateAsync(
        long studentId, StudentNoteUpsertRequest request, CancellationToken ct = default);

    Task<StudentNoteResponse> UpdateAsync(
        long studentId, long id, StudentNoteUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long studentId, long id, CancellationToken ct = default);

    Task<StudentNoteResponse> SetPinnedAsync(long studentId, long id, bool pinned, CancellationToken ct = default);

    Task<StudentNoteResponse> SetColorAsync(long studentId, long id, string? color, CancellationToken ct = default);
}
