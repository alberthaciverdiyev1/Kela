using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Dtos.Auth;
using Kela.App.Shared.Dtos.Progress;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Exams;

public partial class QuizRunner
{
    [Parameter] public long QuizId { get; set; }
    private StartQuizResultDto? _quiz;
    private CompleteQuizResultDto? _result;
    private string? _error;
    private int _index;
    private bool _busy;
    private readonly Dictionary<long, string> _answers = new();

    protected override async Task OnInitializedAsync()
    {
        try { _quiz = await Api.StartQuizAsync(QuizId); }
        catch (Exception ex) { _error = ex.Message; }
    }

    private bool IsSelected(long questionId, string key) => _answers.TryGetValue(questionId, out var a) && a == key;

    private void Prev() { if (_index > 0) _index--; }

    private async Task NextAsync()
    {
        if (_quiz is null) return;
        if (_index < _quiz.Questions.Count - 1) { _index++; return; }

        _busy = true;
        try
        {
            var answers = _quiz.Questions.Select(q =>
                (q.Id, _answers.TryGetValue(q.Id, out var a) ? a : null));
            _result = await Api.CompleteQuizAsync(_quiz.AttemptId, answers);
            Nav.NavigateTo($"/results/{_result.AttemptId}");
        }
        catch (Exception ex) { _error = ex.Message; }
        finally { _busy = false; }
    }
}
