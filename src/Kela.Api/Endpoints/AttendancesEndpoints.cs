using Kela.Api.Contracts;
using Kela.Application.Features.Attendances;
using Kela.Application.Features.Attendances.Requests;
using Kela.Application.Features.Attendances.Responses;
using Kela.Domain.Common;

namespace Kela.Api.Endpoints;

public static class AttendancesEndpoints
{
    public static IEndpointRouteBuilder MapAttendancesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workspaces/{workspaceId:int}/attendance")
            .RequireAuthorization(policy => policy.RequireRole(RoleNames.Teacher, RoleNames.Admin));

        group.MapGet("", async (int workspaceId, DateOnly date, IAttendanceService attendance, CancellationToken ct) =>
            ApiResponse<AttendanceDayResponse>.Success(
                await attendance.GetDayAsync(workspaceId, date, ct)));

        group.MapGet("/month", async (int workspaceId, int year, int month, IAttendanceService attendance, CancellationToken ct) =>
            ApiResponse<AttendanceMonthResponse>.Success(
                await attendance.GetMonthAsync(workspaceId, year, month, ct)));

        group.MapPut("", async (int workspaceId, DateOnly date, SetAttendanceMarksRequest request, IAttendanceService attendance, CancellationToken ct) =>
        {
            await attendance.SetMarksAsync(workspaceId, date, request, ct);
            return ApiResponse.NoContent();
        });

        group.MapDelete("/{studentId:int}", async (int workspaceId, int studentId, DateOnly date, IAttendanceService attendance, CancellationToken ct) =>
        {
            await attendance.ClearMarkAsync(workspaceId, studentId, date, ct);
            return ApiResponse.NoContent();
        });

        return app;
    }
}
