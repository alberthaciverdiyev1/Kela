using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Layout;

public partial class TopNavItem
{
    [Inject] private NavIntent Intent { get; set; } = default!;

    [Parameter] public string Href { get; set; } = "/";
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public string IconName { get; set; } = "book";
    [Parameter] public bool MatchAll { get; set; }

    private string Current => "/" + Nav.ToBaseRelativePath(Nav.Uri).Split('?')[0].TrimEnd('/');

    private bool Active => MatchAll
        ? Current == "/"
        : Current == Href || Current.StartsWith(Href + "/");

    protected override void OnInitialized() => Nav.LocationChanged += OnLocationChanged;

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
        => InvokeAsync(StateHasChanged);

    public void Dispose() => Nav.LocationChanged -= OnLocationChanged;

    private void Open()
    {
        Intent.Next = NavKind.Tab;
        Nav.NavigateTo(Href);
    }

    private string Css =>
        "flex shrink-0 items-center gap-2 rounded-lg px-3 py-2 text-[13px] font-medium transition " +
        (Active ? "bg-brand-073 text-brand-743" : "text-sage-473 hover:bg-brand-041 hover:text-brand-743");
}
