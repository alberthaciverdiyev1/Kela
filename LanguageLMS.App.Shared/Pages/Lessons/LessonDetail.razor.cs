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

public partial class LessonDetail
{
    [Parameter] public long LessonId { get; set; }

    private bool _loading = true;
    private string? Title;
    private List<ContentItemDto> _contents = new();
    private List<QuizDto> _quizzes = new();

    private List<ContentItemDto> _videos => _contents.Where(c => c.Type == "video").ToList();
    private List<ContentItemDto> _files => _contents.Where(c => c.Type == "file").ToList();
    private List<ContentItemDto> _voices => _contents.Where(c => c.Type == "voice").ToList();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            Title = (await Api.LessonAsync(LessonId)).Title;
            _contents = await Api.ContentsAsync(LessonId);
            _quizzes = await Api.LessonQuizzesAsync(LessonId);
        }
        catch { }
        _loading = false;
    }
}
