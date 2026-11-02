namespace Edutor.Core.Modules.Sidebar.Requests;

public sealed class SidebarLabelRequest
{
    public string? Label { get; set; }

    public bool IsActive { get; set; } = true;
}
