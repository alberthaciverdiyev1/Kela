using Microsoft.AspNetCore.Components;

namespace Edutor.App.Shared.Components.Common;

public partial class Modal
{
    [Parameter] public bool Open { get; set; }
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private async Task CloseAsync() => await OnClose.InvokeAsync();
}
