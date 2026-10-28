using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Groups;

public partial class GroupDetail
{
    [Parameter] public long GroupId { get; set; }
    private bool _loading = true;
    private List<SubjectDto> _subjects = new();
    private string? Name;

    protected override void OnInitialized() => OnRefresh(FetchAsync);

    protected override async Task OnInitializedAsync()
    {
        await FetchAsync();
        _loading = false;
    }

    private async Task FetchAsync()
    {
        try
        {
            var groups = await Api.StudentGroupsAsync();
            Name = groups.FirstOrDefault(g => g.Id == GroupId)?.Name;
            _subjects = await Api.SubjectsAsync(GroupId);

            // Qrupda tək fənn varsa, fənn siyahısını göstərmədən birbaşa dərslərə keç
            if (_subjects.Count == 1)
            {
                Nav.NavigateTo($"/subjects/{_subjects[0].Id}", replace: true);
            }
        }
        catch { }
    }
}
