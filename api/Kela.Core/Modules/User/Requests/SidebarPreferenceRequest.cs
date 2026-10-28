namespace Kela.Core.Modules.User.Requests;

/// <summary>İstifadəçinin sidebar-da gizlətdiyi menyu açarları.</summary>
public class SidebarPreferenceRequest
{
    public List<string> Hidden { get; set; } = [];
}
