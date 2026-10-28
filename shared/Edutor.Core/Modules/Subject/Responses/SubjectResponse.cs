namespace Edutor.Core.Modules.Subject.Responses;

public record SubjectResponse(long Id, string Name, long? UserId, bool IsShared);
