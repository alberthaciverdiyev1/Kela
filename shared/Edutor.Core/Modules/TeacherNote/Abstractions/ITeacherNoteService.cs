using Edutor.Core.Common.Responses;
using Edutor.Core.Modules.TeacherNote.Requests;
using Edutor.Core.Modules.TeacherNote.Responses;

namespace Edutor.Core.Modules.TeacherNote.Abstractions;

/// <summary>Şəxsi qeydlər: hər istifadəçi öz qeydlərini yaradır, istəsə seçdiyi həmkarı ilə paylaşır.</summary>
public interface ITeacherNoteService
{
    Task<PagedResponse<TeacherNoteResponse>> ListAsync(long userId, TeacherNoteListRequest request, CancellationToken ct = default);

    Task<TeacherNoteResponse> GetAsync(long userId, long id, CancellationToken ct = default);

    Task<TeacherNoteResponse> CreateAsync(long userId, TeacherNoteUpsertRequest request, CancellationToken ct = default);

    Task<TeacherNoteResponse> UpdateAsync(long userId, long id, TeacherNoteUpsertRequest request, CancellationToken ct = default);

    Task DeleteAsync(long userId, long id, CancellationToken ct = default);

    Task<TeacherNoteResponse> SetPinnedAsync(long userId, long id, bool pinned, CancellationToken ct = default);

    Task<TeacherNoteResponse> SetColorAsync(long userId, long id, string? color, CancellationToken ct = default);

    /// <summary>Qeydi paylaşmaq üçün seçilə bilən həmkar siyahısı (şagirdlər daxil deyil).</summary>
    Task<IReadOnlyList<TeacherNoteUserResponse>> ListShareTargetsAsync(
        long userId, string? search = null, CancellationToken ct = default);

    /// <summary>Qeydə bağlanacaq material seçimləri.</summary>
    Task<IReadOnlyList<TeacherNoteContentOption>> ListContentOptionsAsync(
        string? search = null, CancellationToken ct = default);
}
