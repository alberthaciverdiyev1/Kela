using Microsoft.AspNetCore.Mvc;

namespace Kela.Core.Common.Requests;

/// <summary>Səhifələnən siyahı sorğuları üçün ortaq sahələr (səhifə, ölçü, sıralama).</summary>
public abstract class ListRequestBase
{
    public int Page { get; set; } = 1;

    public int PerPage { get; set; } = 20;

    /// <summary>Sıralama sahəsi: created (standart) | updated | name | order.</summary>
    [FromQuery(Name = "sort_by")]
    public string? SortBy { get; set; }

    /// <summary>Sıralama istiqaməti: desc (standart) | asc.</summary>
    [FromQuery(Name = "sort_dir")]
    public string? SortDir { get; set; }
}
