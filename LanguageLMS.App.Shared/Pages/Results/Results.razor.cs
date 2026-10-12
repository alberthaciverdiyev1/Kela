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

public partial class Results
{
    private bool _loading = true;
    private List<QuizResultSummaryDto> _results = new();

    protected override async Task OnInitializedAsync()
    {
        try { _results = await Api.ResultsAsync(); } catch { }
        _loading = false;
    }
}
