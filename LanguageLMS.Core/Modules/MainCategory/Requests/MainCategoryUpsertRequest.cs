using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.MainCategory.Requests;

public class MainCategoryUpsertRequest
{
    public string Name { get; set; } = string.Empty;

    [FromForm(Name = "user_id")]
    public long? UserId { get; set; }
}
