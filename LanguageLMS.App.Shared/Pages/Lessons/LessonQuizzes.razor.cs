using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Lessons;

public partial class LessonQuizzes
{
    [Parameter] public long LessonId { get; set; }

    private bool _loading = true;
    private string? Title;
    private List<QuizDto> _quizzes = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Title = (await Api.LessonAsync(LessonId)).Title;
            _quizzes = await Api.LessonQuizzesAsync(LessonId);
        }
        catch { }
        _loading = false;
    }
}
