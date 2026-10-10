using Microsoft.AspNetCore.Mvc;

namespace LanguageLMS.Core.Modules.Profession.Requests;

public class ProfessionListRequest
{
    public string? Search { get; set; }

    [FromQuery(Name = "is_deleted")]
    public int IsDeleted { get; set; }

    /// <summary>Verilirse yalnızca bu üst kategorinin alt ixtisasları döner.</summary>
    [FromQuery(Name = "parent_id")]
    public long? ParentId { get; set; }

    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;
}
