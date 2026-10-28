using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Edutor.Core.Modules.Group.Requests;

public class GroupUpsertRequest
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public IFormFile? Image { get; set; }

    public bool RemoveImage { get; set; }

    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }

}
