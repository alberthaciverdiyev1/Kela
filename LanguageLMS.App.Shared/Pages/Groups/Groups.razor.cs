using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Groups;

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
