using FluentValidation;
using Kela.Application.Features.Attendances.Requests;
using Kela.Application.Features.Attendances.Responses;
using Kela.Application.Features.Workspaces;
using Kela.Application.Patterns;
using Kela.Domain.Entities;

namespace Kela.Application.Features.Attendances;

internal sealed class AttendanceService(
    IAttendanceRepository attendance,
    IWorkspaceRepository workspaces,
    IUnitOfWork unitOfWork,
    IValidator<SetAttendanceMarksRequest> marksValidator) : IAttendanceService
{
    public async Task<AttendanceDayResponse> GetDayAsync(
        int workspaceId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var workspace = await workspaces.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {workspaceId} olan iş alanı bulunamadı.");

        var records = await attendance.GetDayAsync(workspaceId, date, cancellationToken);
        return workspace.ToDayResponse(date, records);
    }

    public async Task SetMarksAsync(
        int workspaceId, DateOnly date, SetAttendanceMarksRequest request, CancellationToken cancellationToken = default)
    {
        await marksValidator.ValidateAndThrowAsync(request, cancellationToken);

        var workspace = await workspaces.GetByIdAsync(workspaceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Id = {workspaceId} olan iş alanı bulunamadı.");

        var existing = await attendance.GetDayAsync(workspaceId, date, cancellationToken);
        var byStudent = existing.ToDictionary(r => r.StudentId);

        foreach (var mark in request.Marks)
        {
            if (!workspace.Students.Any(s => s.Id == mark.StudentId))
            {
                throw new InvalidOperationException($"Id = {mark.StudentId} olan öğrenci bu iş alanında değil.");
            }

            if (byStudent.TryGetValue(mark.StudentId, out var record))
            {
                record.Status = mark.Status;
                record.Note = mark.Note;
                record.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                attendance.Add(new Attendance
                {
                    WorkspaceId = workspaceId,
                    StudentId = mark.StudentId,
                    Date = date,
                    Status = mark.Status,
                    Note = mark.Note,
                    CreatedAt = DateTime.UtcNow,
                });
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearMarkAsync(
        int workspaceId, int studentId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var record = await attendance.GetAsync(workspaceId, studentId, date, cancellationToken);
        if (record is not null)
        {
            attendance.Remove(record);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
