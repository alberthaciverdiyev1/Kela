namespace Kela.Core.Modules.Students.Responses;

public record StudentDeviceResponse(long Id, string DeviceId, DateTimeOffset CreatedAt);

public record GeneratedStudentInfo(long Id, string FullName, string Email, string? Password = null);

public record GenerateStudentsResponse(int Created, IReadOnlyList<GeneratedStudentInfo> Students);
