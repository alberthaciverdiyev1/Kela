using Microsoft.AspNetCore.Mvc;

namespace Edutor.Core.Modules.Subject.Requests;

public class SubjectUpsertRequest
{
    public string Name { get; set; } = string.Empty;

    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }

    /// <summary>Fənn bütün müəllimlərə açıq olsun (dərsləri və materialları ilə birlikdə).</summary>
    [FromForm(Name = "is_shared")]
    public bool IsShared { get; set; }

    /// <summary>Yaratma zamanı fənnin əlavə olunacağı qrup (könüllü).</summary>
    [FromForm(Name = "group_id")]
    public long? GroupId { get; set; }

    /// <summary>Mövcud qrup seçilmirsə yeni qrupun adı (könüllü).</summary>
    [FromForm(Name = "new_group_name")]
    public string? NewGroupName { get; set; }
}
