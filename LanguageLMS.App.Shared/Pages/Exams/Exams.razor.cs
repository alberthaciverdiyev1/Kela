using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Exams;

public partial class Exams
{
    private bool _loading = true;
    private List<QuizDto> _quizzes = new();

    protected override async Task OnInitializedAsync()
    {
        try { _quizzes = await Api.QuizzesAsync(); } catch { }
        _loading = false;
    }
}
