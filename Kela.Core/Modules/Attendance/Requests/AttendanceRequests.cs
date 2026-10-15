namespace Kela.Core.Modules.Attendance.Requests;

public class SaveAttendanceRequest
{
    public long GroupId { get; set; }

    public DateOnly Date { get; set; }

    public List<SaveAttendanceItem> Students { get; set; } = [];
}

public class SaveAttendanceItem
{
    public long StudentId { get; set; }

    public bool IsPresent { get; set; }
}
