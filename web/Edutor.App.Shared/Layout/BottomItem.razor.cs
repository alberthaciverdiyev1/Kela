using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Layout;

public partial class BottomItem
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
        "flex min-w-16 flex-col items-center gap-1 text-[10px] font-semibold " +
        (Active ? "text-brand-671" : "text-sage-369");
}
