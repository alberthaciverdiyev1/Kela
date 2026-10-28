using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Components.Common;

public partial class PageHeader
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private NavIntent Intent { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Back { get; set; } = "/";
    [Parameter] public bool Sticky { get; set; }
    [Parameter] public RenderFragment? Right { get; set; }

    private async Task GoBack()
    {
        Intent.Next = NavKind.Back;

        // "javascript:history.back()" — brauzer özü geri gedir; niyyət artıq qeyd
        // olunduğu üçün popstate-dan sonra animasiya düzgün seçilir.
        if (Back.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
        {
            await JS.InvokeVoidAsync("history.back");
            return;
        }

        Nav.NavigateTo(Back);
    }
}
