using Kela.Domain.Common;

namespace Kela.Application.Features.Attendances.Requests;

public sealed record AttendanceMarkRequest(int StudentId, AttendanceStatus Status, string? Note);
