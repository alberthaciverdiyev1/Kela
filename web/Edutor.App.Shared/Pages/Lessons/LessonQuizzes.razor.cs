using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Lessons;

public partial class LessonQuizzes
{
    [Parameter] public long LessonId { get; set; }

    private bool _loading = true;
    private string? Title;
    private List<QuizDto> _quizzes = new();

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
            Title = (await Api.LessonAsync(LessonId)).Title;
            _quizzes = await Api.LessonQuizzesAsync(LessonId);
        }
        catch { }
    }
}
