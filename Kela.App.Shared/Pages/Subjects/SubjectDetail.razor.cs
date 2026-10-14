using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Subjects;

public partial class SubjectDetail
{
    [Parameter] public long SubjectId { get; set; }
    private bool _loading = true;
    private string? Name;
    private List<LessonDto> _lessons = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Name = (await Api.SubjectAsync(SubjectId)).Name;
            _lessons = await Api.LessonsAsync(SubjectId);
        }
        catch { }
        _loading = false;
    }
}
