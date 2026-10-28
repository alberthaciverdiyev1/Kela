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

public partial class EmptyState
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public string Class { get; set; } = "mt-6 text-sm text-sage-410";
}
