using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Components.Groups;

public partial class GroupCard
{
    [Parameter, EditorRequired] public GroupDto Group { get; set; } = default!;
    [Parameter] public int Index { get; set; }

    private string Tone => (Index % 4) switch { 0 => "bg-brand-083", 1 => "bg-sky-061", 2 => "bg-sun-069", _ => "bg-grape-051" };
    private string ToneText => (Index % 4) switch { 0 => "text-brand-698", 1 => "text-sky-531", 2 => "text-sun-529", _ => "text-grape-447" };
}
