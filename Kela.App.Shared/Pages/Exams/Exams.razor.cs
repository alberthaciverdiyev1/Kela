using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Exams;

public partial class Exams
{
    private bool _loading = true;
    private List<QuizDto> _quizzes = new();

    // "general" (default) | "all" | "lesson"
    private string _mode = "general";
    private long? _lessonId;

    private IEnumerable<(long Id, string Title)> Lessons =>
        _quizzes.Where(q => q.LessonId is not null)
            .GroupBy(q => q.LessonId!.Value)
            .Select(g => (g.Key, g.First().LessonTitle ?? $"#{g.Key}"));

    private List<QuizDto> Filtered => _mode switch
    {
        "all" => _quizzes,
        "lesson" => _quizzes.Where(q => q.LessonId == _lessonId).ToList(),
        _ => _quizzes.Where(q => q.LessonId is null).ToList()
    };

    protected override async Task OnInitializedAsync()
    {
        try { _quizzes = await Api.QuizzesAsync(); } catch { }
        _lessonId = Lessons.Select(l => (long?)l.Id).FirstOrDefault();
        _loading = false;
    }

    private void SetMode(string mode)
    {
        _mode = mode;
        if (mode == "lesson" && _lessonId is null)
        {
            _lessonId = Lessons.Select(l => (long?)l.Id).FirstOrDefault();
        }
    }
}
