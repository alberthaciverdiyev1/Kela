using Kela.App.Shared.Dtos;
using Kela.App.Shared.Services;
using Kela.App.Shared.Services.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Kela.App.Shared.Components.Feedback;

public partial class FeedbackButton
{
    [Inject] private ApiClient Api { get; set; } = default!;

    private bool _open;
    private bool _busy;
    private bool _sent;
    private bool _error;
    private string? _errorMessage;
    private string _message = "";
    private IBrowserFile? _file;

    private void OpenAsync()
    {
        _open = true;
        _sent = false;
        _error = false;
        _errorMessage = null;
    }

    private void CloseAsync() => _open = false;

    private void OnFile(InputFileChangeEventArgs e) => _file = e.File;

    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(_message))
        {
            _error = true;
            _errorMessage = L["Feedback_Placeholder"];
            return;
        }

        _busy = true;
        _error = false;

        try
        {
            Stream? stream = _file is null ? null : _file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
            await Api.PostFeedbackAsync<FeedbackDto>("/api/feedback", _message.Trim(), stream, _file?.Name, _file?.ContentType);

            _sent = true;
            _message = "";
            _file = null;
            await Task.Delay(1200);
            _open = false;
        }
        catch (Exception ex)
        {
            _error = true;
            _errorMessage = ex.Message;
        }
        finally
        {
            _busy = false;
        }
    }
}
