namespace Edutor.Core.Modules.Sidebar.Responses;

public sealed class SidebarSummaryResponse
{
    public Dictionary<string, int> Counts { get; set; } = [];

    /// <summary>Diqqət tələb edən və ya son baxışdan sonra əlavə olunan bölmələr.</summary>
    public List<string> NewKeys { get; set; } = [];

    /// <summary>Sayğacı zəng + qırmızı nişan kimi göstərilən bölmələr (müraciət, mesaj, bildiriş).</summary>
    public List<string> AlertKeys { get; set; } = [];

    /// <summary>Superadmin tərəfindən təyin olunmuş etiketlər: açar → mətn.</summary>
    public Dictionary<string, string> Labels { get; set; } = [];
}

public sealed class SidebarLabelResponse
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}
