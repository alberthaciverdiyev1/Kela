using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.TeacherNote.Requests;
using Edutor.Core.Modules.TeacherNote.Responses;

namespace Edutor.Core.Modules.TeacherNote.Abstractions;

/// <summary>Şəxsi qeydlər: hər istifadəçi yalnız öz qeydlərini görür və dəyişir.</summary>
public interface ITeacherNoteService
{
    Task<PagedResponse<TeacherNoteResponse>> ListAsync(long userId, TeacherNoteListRequest request, CancellationToken ct = default);

    Task<TeacherNoteResponse> GetAsync(long userId, long id, CancellationToken ct = default);

    Task<TeacherNoteResponse> CreateAsync(long userId, TeacherNoteUpsertRequest request, CancellationToken ct = default);

    Task<TeacherNoteResponse> UpdateAsync(long userId, long id, TeacherNoteUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long userId, long id, CancellationToken ct = default);
}
