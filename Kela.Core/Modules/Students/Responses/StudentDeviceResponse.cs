namespace Kela.Core.Modules.Students.Responses;

public record StudentDeviceResponse(long Id, string DeviceId, DateTimeOffset CreatedAt);

public record GeneratedStudentInfo(long Id, string FullName, string Email);

public record GenerateStudentsResponse(int Created, bool CredentialsSent, IReadOnlyList<GeneratedStudentInfo> Students);
