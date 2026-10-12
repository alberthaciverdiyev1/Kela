using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using LanguageLMS.App.Shared.Dtos;
using LanguageLMS.App.Shared.Dtos.Auth;
using LanguageLMS.App.Shared.Dtos.Progress;
using LanguageLMS.App.Shared.Dtos.Quiz;
using LanguageLMS.App.Shared.Services;
using LanguageLMS.App.Shared.Services.Api;

namespace LanguageLMS.App.Shared.Pages.Documents;

public partial class DocumentViewer
{
    [Parameter, SupplyParameterFromQuery(Name = "url")] public string? UrlQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "title")] public string? TitleQuery { get; set; }

    private string? _url;
    private string? _title;
    private ElementReference _pdfRef;
    private string? _attached;

    protected override void OnParametersSet()
    {
        _url = UrlQuery;
        _title = TitleQuery;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!string.IsNullOrEmpty(_url) && _attached != _url)
        {
            _attached = _url;
            await JS.InvokeVoidAsync("lmsPdf.render", _pdfRef, _url);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { await JS.InvokeVoidAsync("lmsPdf.clear", _pdfRef); } catch { /* noop */ }
    }
}
