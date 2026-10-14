using Microsoft.AspNetCore.Components;
using Kela.App.Shared.Dtos;
using Kela.App.Shared.Services;

namespace Kela.App.Shared.Components.Lessons;

public partial class ContentCard
{
    [Parameter, EditorRequired] public ContentItemDto Content { get; set; } = default!;
    [Parameter] public long LessonId { get; set; }

    private bool IsVideo => Content.Type == "video";
    private bool IsVoice => Content.Type == "voice";

    private string Meta => $"{Content.TypeLabel}{(!string.IsNullOrEmpty(Content.DurationLabel) ? $" · {Content.DurationLabel}" : "")}";

    private string Url
    {
        get
        {
            var url = Content.PlaybackUrl ?? Content.Url ?? "";
            return url.StartsWith("http") ? url : $"{AppConfig.ApiBaseUrl}/{url.TrimStart('/')}";
        }
    }

    private string Preview => string.IsNullOrEmpty(Url) ? "" : Url + "#t=0.1";

    private Task ActAsync()
    {
        if (IsVoice)
        {
            Player.Play(Url, Content.Title);
            return Task.CompletedTask;
        }

        // pdf və ya digər fayl → tam səhifə sənəd görüntüləyicisi
        Nav.NavigateTo($"/document?url={Uri.EscapeDataString(Url)}&title={Uri.EscapeDataString(Content.Title)}");
        return Task.CompletedTask;
    }
}
