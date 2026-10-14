using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Groups;

public partial class Groups
{
    private bool _loading = true;
    private List<GroupDto> _groups = new();

    protected override async Task OnInitializedAsync()
    {
        try { _groups = await Api.StudentGroupsAsync(); } catch { }
        _loading = false;
    }
}
