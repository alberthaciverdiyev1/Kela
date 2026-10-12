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

public partial class GroupDetail
{
    [Parameter] public long GroupId { get; set; }
    private bool _loading = true;
    private List<SubjectDto> _subjects = new();
    private string? Name;

    protected override async Task OnInitializedAsync()
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
                return;
            }
        }
        catch { }
        _loading = false;
    }
}
