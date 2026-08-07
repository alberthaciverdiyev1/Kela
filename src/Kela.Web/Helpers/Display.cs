namespace Kela.Web.Helpers;

public static class Display
{
    public static string Render(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;

    public static string Render(DateOnly? value) =>
        value?.ToString("dd.MM.yyyy") ?? "-";
}
