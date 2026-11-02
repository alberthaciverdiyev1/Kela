namespace Edutor.Core.Common.Notes;

/// <summary>Qeyd kartının rəngi — klientdəki palitra ilə eyni siyahı.</summary>
public static class NoteColors
{
    public const string Default = "default";

    public static readonly string[] All =
    [
        Default, "red", "orange", "yellow", "green", "teal", "blue", "purple", "pink", "gray", "brown"
    ];

    /// <summary>Naməlum dəyər default-a düşür — klientdən gələn rəng sərbəst mətn deyil.</summary>
    public static string Normalize(string? color)
        => color is not null && All.Contains(color) ? color : Default;
}
