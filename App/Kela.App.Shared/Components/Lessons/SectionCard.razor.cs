using Microsoft.AspNetCore.Components;

namespace Kela.App.Shared.Components.Lessons;

public partial class SectionCard
{
    [Parameter, EditorRequired] public string Icon { get; set; } = string.Empty;
    [Parameter, EditorRequired] public string Title { get; set; } = string.Empty;
    [Parameter] public string? Subtitle { get; set; }
    [Parameter, EditorRequired] public string Href { get; set; } = string.Empty;
    [Parameter] public string Tone { get; set; } = "bg-brand-081 text-brand-698";
}
