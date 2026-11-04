using Microsoft.AspNetCore.Components;
using Edutor.App.Shared.Dtos;
using Edutor.App.Shared.Services;

namespace Edutor.App.Shared.Components.Lessons;

public partial class ContentCard
{
    [Parameter, EditorRequired] public ContentItemDto Content { get; set; } = default!;
    [Parameter] public long LessonId { get; set; }

    /// <summary>Dərsin bütün səsləri — pleyer bunlar arasında avtomatik keçid edir.</summary>
    [Parameter] public IReadOnlyList<ContentItemDto>? VoicePlaylist { get; set; }

    private bool IsVideo => Content.Type == "video";
    private bool IsVoice => Content.Type == "voice";

    private string Meta => $"{L.TypeLabel(Content.Type)}{(!string.IsNullOrEmpty(Content.DurationLabel) ? $" · {Content.DurationLabel}" : "")}";

    private string Url
    {
        get
        {
            var url = Content.PlaybackUrl ?? Content.Url ?? "";
            return url.StartsWith("http") ? url : $"{AppConfig.ApiBaseUrl}/{url.TrimStart('/')}";
        }
    }

    private string Preview => string.IsNullOrEmpty(Url) ? "" : Url + "#t=0.1";

    private static AudioTrack Absolute(ContentItemDto c)
    {
        var url = c.PlaybackUrl ?? c.Url ?? "";
        return new AudioTrack(
            url.StartsWith("http") ? url : $"{AppConfig.ApiBaseUrl}/{url.TrimStart('/')}",
            c.Title,
            c.Id);
    }

    private Task ActAsync()
    {
        if (IsVoice)
        {
            if (VoicePlaylist is { Count: > 0 } list)
            {
                var index = list.ToList().FindIndex(c => c.Id == Content.Id);
                Player.Play(list.Select(Absolute).ToList(), index < 0 ? 0 : index);
            }
            else
            {
                Player.Play(Url, Content.Title);
            }
            return Task.CompletedTask;
        }

        // pdf və ya digər fayl → tam səhifə sənəd görüntüləyicisi
        Nav.NavigateTo($"/document?url={Uri.EscapeDataString(Url)}&title={Uri.EscapeDataString(Content.Title)}");
        return Task.CompletedTask;
    }
}
