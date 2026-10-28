using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Dtos.Auth;
using Edutor.App.Shared.Dtos.Progress;
using Edutor.App.Shared.Dtos.Quiz;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Api;

namespace Edutor.App.Shared.Pages.Documents;

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
            await JS.InvokeVoidAsync("lmsPdf.render", _pdfRef, _url, L["Document_LoadFailed"]);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try { await JS.InvokeVoidAsync("lmsPdf.clear", _pdfRef); } catch { /* noop */ }
    }
}
