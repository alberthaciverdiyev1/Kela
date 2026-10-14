using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Subject.Requests;

public class SubjectUpsertRequest
{
    public string Name { get; set; } = string.Empty;

    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }
}
