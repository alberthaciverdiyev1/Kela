using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Exams;

public partial class Exams
{
    private bool _loading = true;
    private bool _busy;
    private List<QuizDto> _quizzes = new();
    private List<SubjectDto> _subjects = new();
    private List<LessonDto> _lessons = new();

    private long? _subjectId;
    private long? _lessonId;
    private bool _onlyGeneral = true;   // default: ümumi imtahanlar

    protected override async Task OnInitializedAsync()
    {
        try { _subjects = await Api.AllSubjectsAsync(); } catch { }
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _busy = true;
        try
        {
            _quizzes = _onlyGeneral
                ? await Api.QuizzesAsync(general: true)
                : await Api.QuizzesAsync(_subjectId, _lessonId);
        }
        catch { _quizzes = new(); }
        finally { _busy = false; _loading = false; }
    }

    private async Task SetModeAsync(bool general)
    {
        _onlyGeneral = general;
        if (general)
        {
            _subjectId = null;
            _lessonId = null;
            _lessons = new();
        }
        await ReloadAsync();
    }

    private async Task OnSubjectChangedAsync(ChangeEventArgs e)
    {
        _subjectId = long.TryParse(e.Value?.ToString(), out var id) && id > 0 ? id : null;
        _lessonId = null;
        _lessons = new();

        if (_subjectId is long sid)
        {
            try { _lessons = await Api.LessonsAsync(sid); } catch { }
        }

        await ReloadAsync();
    }

    private async Task OnLessonChangedAsync(ChangeEventArgs e)
    {
        _lessonId = long.TryParse(e.Value?.ToString(), out var id) && id > 0 ? id : null;
        await ReloadAsync();
    }
}
