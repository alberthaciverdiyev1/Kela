using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Subjects;

public partial class SubjectDetail
{
    [Parameter] public long SubjectId { get; set; }
    private bool _loading = true;
    private string? Name;
    private List<LessonDto> _lessons = new();

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
            Name = (await Api.SubjectAsync(SubjectId)).Name;
            _lessons = await Api.LessonsAsync(SubjectId);
        }
        catch { }
    }
}
