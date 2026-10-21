using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Subject.Requests;

public class SubjectUpsertRequest
{
    public string Name { get; set; } = string.Empty;

    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }

    /// <summary>Fənn bütün müəllimlərə açıq olsun (dərsləri və materialları ilə birlikdə).</summary>
    [FromForm(Name = "is_shared")]
    public bool IsShared { get; set; }
}
