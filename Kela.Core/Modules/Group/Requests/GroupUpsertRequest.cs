using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Modules.Group.Requests;

public class GroupUpsertRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }

    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }

    /// <summary>Qrupla birlikdə şagird generasiya edilsin? Arxa planda işə düşür.</summary>
    [FromForm(Name = "generate_count")]
    public int GenerateCount { get; set; }

    [FromForm(Name = "generate_prefix")]
    public string? GeneratePrefix { get; set; }

    [FromForm(Name = "generate_domain")]
    public string? GenerateDomain { get; set; }

    [FromForm(Name = "generate_password")]
    public string? GeneratePassword { get; set; }
}
