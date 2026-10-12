using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Results;

public partial class ResultsDetail
{
    [Parameter] public long AttemptId { get; set; }
    private bool _loading = true;
    private QuizResultDetailDto? _detail;

    protected override async Task OnInitializedAsync()
    {
        try { _detail = await Api.ResultDetailAsync(AttemptId); } catch { }
        _loading = false;
    }
}
