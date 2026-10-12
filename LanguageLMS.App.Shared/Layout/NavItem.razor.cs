using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Layout;

public partial class NavItem
{
    [Parameter] public string Href { get; set; } = "/";
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string IconName { get; set; } = "book";
    [Parameter] public bool MatchAll { get; set; }

    private string Current => "/" + Nav.ToBaseRelativePath(Nav.Uri).Split('?')[0].TrimEnd('/');

    private bool Active => MatchAll
        ? Current == "/"
        : Current == Href || Current.StartsWith(Href + "/");

    private string Css =>
        "flex items-center gap-3 rounded-lg px-3 py-2.5 text-left text-[13px] font-medium transition " +
        (Active ? "bg-brand-073 text-brand-743" : "text-sage-473 hover:bg-brand-041");
}
