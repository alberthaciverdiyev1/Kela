using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Subjects;

public partial class SubjectDetail
{
    [Parameter] public long SubjectId { get; set; }
    private bool _loading = true;
    private List<LessonDto> _lessons = new();

    protected override async Task OnInitializedAsync()
    {
        try { _lessons = await Api.LessonsAsync(SubjectId); } catch { }
        _loading = false;
    }
}
