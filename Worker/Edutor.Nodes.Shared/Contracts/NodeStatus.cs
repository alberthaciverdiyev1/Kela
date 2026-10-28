namespace Edutor.Nodes.Shared.Contracts;

public static class NodeStatus
{
    public const string Online = "online";
    public const string Offline = "offline";

    /// <summary>Admin tərəfindən əl ilə dayandırılıb — tapşırıq verilmir.</summary>
    public const string Disabled = "disabled";

    public static readonly string[] All = [Online, Offline, Disabled];
}
