using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Components.Common;

public partial class PageHeader
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Back { get; set; } = "/";
    [Parameter] public bool Sticky { get; set; }
    [Parameter] public RenderFragment? Right { get; set; }
}
