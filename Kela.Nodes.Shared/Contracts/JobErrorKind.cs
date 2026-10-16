namespace Kela.Nodes.Shared.Contracts;

/// <summary>
/// Xətanın növü — retry siyasətini müəyyən edir. Şəbəkə/disk xətaları təkrar cəhdə
/// dəyər; "bu fayl korlanıb" kimi xətalar dəyməz.
/// </summary>
public static class JobErrorKind
{
    public const string Transient = "transient";
    public const string Permanent = "permanent";

    public static readonly string[] All = [Transient, Permanent];
}
