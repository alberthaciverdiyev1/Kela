using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Kela.Core.Modules.Attendance.Abstractions;
using Kela.Core.Modules.Attendance.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace Kela.Tests.Attendance;

[Collection("api")]
public class AttendanceTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.TeacherClient();

    private static string Unique(string prefix) => $"{prefix}-{Guid.NewGuid():N}";

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage r)
        => (await JsonDocument.ParseAsync(await r.Content.ReadAsStreamAsync())).RootElement.Clone();

    private async Task<(long id, string token)> RegisterStudentAsync()
    {
        var r = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            fullName = "Student",
            email = $"student-{Guid.NewGuid():N}@example.com",
            password = "secret123"
        });
        var data = (await ReadJsonAsync(r)).GetProperty("data");
        return (data.GetProperty("user").GetProperty("id").GetInt64(), data.GetProperty("token").GetString()!);
    }

    private async Task<T> WithServiceAsync<T>(Func<IAttendanceService, Task<T>> action)
    {
        using var scope = factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<IAttendanceService>());
    }

    [Fact]
    public async Task Save_day_then_read_day_month_and_student_summary()
    {
        var (studentId, token) = await RegisterStudentAsync();
        var groupId = await TestData.GroupAsync(factory, Unique("Group"));
        await TestData.AssignStudentsAsync(factory, groupId, [studentId]);

        var date = new DateOnly(2026, 3, 4);

        // Qeyd yoxdur: bütün üzvlər "gəlməyib" sayılır.
        var before = await WithServiceAsync(s => s.GetDayAsync(groupId, date));
        Assert.Equal(1, before.TotalCount);
        Assert.Equal(0, before.PresentCount);
        Assert.False(before.Students.Single().IsPresent);

        // İştirakı qeyd et.
        var saved = await WithServiceAsync(s => s.SaveDayAsync(new SaveAttendanceRequest
        {
            GroupId = groupId,
            Date = date,
            Students = [new SaveAttendanceItem { StudentId = studentId, IsPresent = true }]
        }));
        Assert.Equal(1, saved.PresentCount);
        Assert.True(saved.Students.Single().IsPresent);

        // Ay xülasəsi.
        var month = await WithServiceAsync(s => s.GetMonthAsync(groupId, 2026, 3));
        var day = Assert.Single(month.Days);
        Assert.Equal(date, day.Date);
        Assert.Equal(1, day.PresentCount);
        Assert.Equal(1, day.TotalCount);

        // Eyni günü yenidən yazmaq — upsert olmalı, təkrar sətir yaranmamalıdır.
        await WithServiceAsync(s => s.SaveDayAsync(new SaveAttendanceRequest
        {
            GroupId = groupId,
            Date = date,
            Students = [new SaveAttendanceItem { StudentId = studentId, IsPresent = false }]
        }));
        var after = await WithServiceAsync(s => s.GetDayAsync(groupId, date));
        Assert.Equal(0, after.PresentCount);

        var monthAfter = await WithServiceAsync(s => s.GetMonthAsync(groupId, 2026, 3));
        Assert.Single(monthAfter.Days);

        // Şagird öz xülasəsini API-dən görür.
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var res = await client.GetAsync("/api/attendance/my");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var summary = (await ReadJsonAsync(res)).GetProperty("data");
        Assert.Equal(1, summary.GetProperty("totalCount").GetInt32());
        Assert.Equal(0, summary.GetProperty("presentCount").GetInt32());
        Assert.Equal(1, summary.GetProperty("absentCount").GetInt32());
    }

    [Fact]
    public async Task Only_group_members_are_recorded()
    {
        var (studentId, _) = await RegisterStudentAsync();
        var (outsiderId, _) = await RegisterStudentAsync();

        var groupId = await TestData.GroupAsync(factory, Unique("Group"));
        await TestData.AssignStudentsAsync(factory, groupId, [studentId]);

        var date = new DateOnly(2026, 4, 6);

        // Qrup üzvü olmayan şagird sorğuda gəlsə də qeyd olunmamalıdır.
        await WithServiceAsync(s => s.SaveDayAsync(new SaveAttendanceRequest
        {
            GroupId = groupId,
            Date = date,
            Students =
            [
                new SaveAttendanceItem { StudentId = studentId, IsPresent = true },
                new SaveAttendanceItem { StudentId = outsiderId, IsPresent = true }
            ]
        }));

        var day = await WithServiceAsync(s => s.GetDayAsync(groupId, date));
        Assert.Equal(1, day.TotalCount);
        Assert.Equal(1, day.PresentCount);

        var outsider = await WithServiceAsync(s => s.GetStudentSummaryAsync(outsiderId));
        Assert.Equal(0, outsider.TotalCount);
    }

    [Fact]
    public async Task Unknown_group_is_rejected()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => WithServiceAsync(s => s.GetDayAsync(999_999, new DateOnly(2026, 3, 4))));
    }
}
