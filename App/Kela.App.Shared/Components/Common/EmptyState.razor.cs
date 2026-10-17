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

public partial class EmptyState
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public string Class { get; set; } = "mt-6 text-sm text-sage-410";
}
