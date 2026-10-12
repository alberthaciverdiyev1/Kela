using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Components.Common;

public partial class PageHeader
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Back { get; set; } = "/";
    [Parameter] public bool Sticky { get; set; }
    [Parameter] public RenderFragment? Right { get; set; }
}
