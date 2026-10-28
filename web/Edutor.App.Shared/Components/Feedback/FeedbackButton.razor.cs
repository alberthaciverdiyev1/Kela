using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Services;
using Edutor.App.Shared.Services.Localization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR.Client;

namespace Edutor.App.Shared.Components.Feedback;

public partial class FeedbackButton : IAsyncDisposable
{
    [Inject] private ApiClient Api { get; set; } = default!;

    private bool _open;
    private bool _busy;
    private bool _loading;
    private bool _hasReply;
    private bool _error;
    private string? _errorMessage;
    private string _message = "";
    private IBrowserFile? _file;
    private long? _threadId;
    private readonly List<FeedbackMessageDto> _messages = new();
    private HubConnection? _hub;

    private void OpenAsync()
    {
        _open = true;
        _error = false;
        _errorMessage = null;
        _ = LoadAsync();
    }

    private void CloseAsync() => _open = false;

    private void OnFile(InputFileChangeEventArgs e) => _file = e.File;

    private async Task LoadAsync()
    {
        _loading = true;
        try
        {
            var page = await Api.GetAsync<PagedFeedbackThreads>("api/feedback/mine");
            var thread = page?.Data?.FirstOrDefault();
            _messages.Clear();
            _threadId = thread?.Id;
            if (thread is not null)
            {
                _messages.AddRange(thread.Messages);
                await ConnectHubAsync();
                await MarkReadAsync();
            }
            _hasReply = false;
        }
        catch
        {
            // Söhbət yüklənməzsə də yeni mesaj göndərmək mümkündür.
        }
        finally
        {
            _loading = false;
            // LoadAsync açılışdan fire-and-forget çağırılır — render özü təzələnmir.
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(_message)) return;

        _busy = true;
        _error = false;

        try
        {
            Stream? stream = _file is null ? null : _file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);

            FeedbackThreadDto? thread;
            if (_threadId is long id)
                thread = await Api.PostFeedbackMessageAsync<FeedbackThreadDto>($"api/feedback/{id}/messages", _message.Trim(), stream, _file?.Name, _file?.ContentType);
            else
                thread = await Api.PostFeedbackAsync<FeedbackThreadDto>("api/feedback", _message.Trim(), stream, _file?.Name, _file?.ContentType);

            if (thread is not null)
            {
                _threadId = thread.Id;
                _messages.Clear();
                _messages.AddRange(thread.Messages);
                await ConnectHubAsync();
            }

            _message = "";
            _file = null;
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

    private async Task MarkReadAsync()
    {
        if (_threadId is not long id) return;
        try { await Api.PostAsync<object>($"api/feedback/{id}/read", new { }); } catch { }
    }

    private async Task ConnectHubAsync()
    {
        if (_hub is not null || string.IsNullOrWhiteSpace(Api.Token)) return;

        // Hub API hostundadır — app origin-i (Nav.BaseUri) deyil.
        _hub = new HubConnectionBuilder()
            .WithUrl($"{AppConfig.ApiBaseUrl}/hubs/feedback", options => options.AccessTokenProvider = () => Task.FromResult(Api.Token))
            .WithAutomaticReconnect()
            .Build();

        _hub.On<FeedbackThreadDto>("feedback:message", async (thread) =>
        {
            if (_threadId is long id && thread.Id == id)
            {
                _messages.Clear();
                _messages.AddRange(thread.Messages);
                await InvokeAsync(StateHasChanged);
            }
            else if (_threadId is null)
            {
                _hasReply = true;
                await InvokeAsync(StateHasChanged);
            }
        });

        try { await _hub.StartAsync(); } catch { /* hub əlçatmazsa */ }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hub is not null) await _hub.DisposeAsync();
    }

    private sealed record PagedFeedbackThreads(List<FeedbackThreadDto>? Data);
}
