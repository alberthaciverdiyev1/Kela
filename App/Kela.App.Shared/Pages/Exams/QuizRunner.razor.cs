using Microsoft.AspNetCore.Components;
using System.Threading;
using Kela.App.Shared.Dtos.Quiz;
using Kela.App.Shared.Services.Api;

namespace Kela.App.Shared.Pages.Exams;

public partial class QuizRunner : IDisposable
{
    [Parameter] public long QuizId { get; set; }
    private StartQuizResultDto? _quiz;
    private CompleteQuizResultDto? _result;
    private string? _error;
    private int _index;
    private bool _busy;
    private readonly Dictionary<long, string> _answers = new();

    private Timer? _timer;
    private int _remaining;
    private bool _timeUp;

    protected override async Task OnInitializedAsync()
    {
        try { _quiz = await Api.StartQuizAsync(QuizId); }
        catch (Exception ex) { _error = ex.Message; }

        if (_quiz is { DurationMinutes: > 0 })
        {
            _remaining = _quiz.DurationMinutes * 60;
            _timer = new Timer(_ => Tick(), null, 1000, 1000);
        }
    }

    private void Tick()
    {
        if (_remaining <= 0) return;

        _remaining--;
        if (_remaining <= 0)
        {
            _timeUp = true;
            StopTimer();
        }

        _ = InvokeAsync(StateHasChanged);
    }

    private void StopTimer() => _timer?.Change(Timeout.Infinite, Timeout.Infinite);

    private static string FormatTime(int seconds)
        => $"{seconds / 60:00}:{seconds % 60:00}";

    private bool IsSelected(long questionId, string key) => _answers.TryGetValue(questionId, out var a) && a == key;

    private void Prev() { if (_index > 0) _index--; }

    private async Task NextAsync()
    {
        if (_quiz is null) return;
        if (_index < _quiz.Questions.Count - 1) { _index++; return; }

        await SubmitAsync();
    }

    /// <summary>Vaxt bitdikdə modalı bağlayıb avtomatik təqdim edir.</summary>
    private async Task CloseTimeUpAndSubmitAsync()
    {
        _timeUp = false;
        StateHasChanged();
        await SubmitAsync();
    }

    private async Task SubmitAsync()
    {
        if (_quiz is null || _busy) return;

        StopTimer();
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

    public void Dispose() => _timer?.Dispose();
}
