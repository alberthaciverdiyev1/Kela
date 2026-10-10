using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.Profession.Requests;

public class ProfessionUpsertRequest
{
    public string Name { get; set; } = string.Empty;

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }

    [FromForm(Name = "parent_id")]
    public long? ParentId { get; set; }

    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }
}
