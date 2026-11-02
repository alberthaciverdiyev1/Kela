namespace Edutor.Core.Modules.Sidebar.Entities;

/// <summary>
/// Sidebar bölməsinin yanında göstərilən platforma səviyyəli etiket (məs. "Yeni").
/// Tenant-siz saxlanılır: superadmin yazır, bütün tenant-lar oxuyur.
/// </summary>
public class SidebarLabel
{
    public long Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
