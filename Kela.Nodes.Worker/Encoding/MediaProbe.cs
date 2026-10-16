using System.Globalization;
using System.Text.Json;
using Kela.Nodes.Shared.Contracts;

namespace Kela.Nodes.Worker.Encoding;

public sealed record MediaStreamInfo(
    int Index,
    string CodecType,
    string? CodecName,
    int Width,
    int Height,
    double Fps,
    bool IsAttachedPicture);

public sealed record MediaInfo(
    int Width,
    int Height,
    double Fps,
    int DurationSeconds,
    bool HasAudio,
    int VideoStreamIndex,
    int AudioStreamIndex,
    string? VideoCodec,
    string? AudioCodec,
    bool HasSubtitles,
    bool HasAttachments)
{
    public SourceMediaInfo ToSourceMediaInfo() => new()
    {
        Width = Width,
        Height = Height,
        Fps = Fps > 0 ? Fps : null,
        DurationSeconds = DurationSeconds > 0 ? DurationSeconds : null,
        HasAudio = HasAudio
    };
}

/// <summary>ffprobe ilə mənbə faylın strukturunu oxuyur.</summary>
public sealed class MediaProbe(string ffprobePath)
{
    public async Task<MediaInfo?> ProbeAsync(string file, CancellationToken ct = default)
    {
        var result = await ProcessRunner.RunAsync(ffprobePath,
        [
            "-hide_banner", "-loglevel", "error",
            "-print_format", "json",
            "-show_format", "-show_streams",
            file
        ], timeout: TimeSpan.FromMinutes(2), ct: ct);

        if (!result.Success) return null;

        try
        {
            using var document = JsonDocument.Parse(result.Output);
            return Parse(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static MediaInfo Parse(JsonElement root)
    {
        var streams = new List<MediaStreamInfo>();

        if (root.TryGetProperty("streams", out var streamArray))
        {
            foreach (var stream in streamArray.EnumerateArray())
            {
                streams.Add(new MediaStreamInfo(
                    Index: GetInt(stream, "index") ?? 0,
                    CodecType: GetString(stream, "codec_type") ?? string.Empty,
                    CodecName: GetString(stream, "codec_name"),
                    Width: GetInt(stream, "width") ?? 0,
                    Height: GetInt(stream, "height") ?? 0,
                    Fps: ParseFps(GetString(stream, "r_frame_rate") ?? GetString(stream, "avg_frame_rate")),
                    IsAttachedPicture: IsAttachedPicture(stream)));
            }
        }

        // Albom qapağı (attached_pic) video axını kimi görünür — onu real video saymırıq.
        var video = streams.FirstOrDefault(s => s.CodecType == "video" && !s.IsAttachedPicture);
        var audio = streams.FirstOrDefault(s => s.CodecType == "audio");

        var duration = ParseDouble(GetString(root, "format", "duration"));

        return new MediaInfo(
            Width: video?.Width ?? 0,
            Height: video?.Height ?? 0,
            Fps: video?.Fps ?? 0,
            DurationSeconds: (int)Math.Round(duration),
            HasAudio: audio is not null,
            // Real video axınının indeksi — "0:v:0" albom qapağını seçə bilər.
            VideoStreamIndex: video?.Index ?? 0,
            AudioStreamIndex: audio?.Index ?? -1,
            VideoCodec: video?.CodecName,
            AudioCodec: audio?.CodecName,
            HasSubtitles: streams.Any(s => s.CodecType == "subtitle"),
            HasAttachments: streams.Any(s => s.CodecType == "attachment"));
    }

    /// <summary>Albom qapağı kimi əlavə olunmuş şəkillər video axını deyil.</summary>
    private static bool IsAttachedPicture(JsonElement stream)
        => stream.TryGetProperty("disposition", out var disposition)
           && disposition.TryGetProperty("attached_pic", out var attached)
           && attached.ValueKind == JsonValueKind.Number
           && attached.GetInt32() == 1;

    private static string? GetString(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? GetString(JsonElement element, string parent, string property)
        => element.TryGetProperty(parent, out var inner) ? GetString(inner, property) : null;

    private static int? GetInt(JsonElement element, string property)
    {
        if (!element.TryGetProperty(property, out var value)) return null;

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetInt32(out var number) ? number : null,
            JsonValueKind.String => int.TryParse(value.GetString(), CultureInfo.InvariantCulture, out var parsed) ? parsed : null,
            _ => null
        };
    }

    private static double ParseDouble(string? value)
        => double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    /// <summary>"30000/1001" kimi kəsr FPS dəyərini ədədə çevirir.</summary>
    private static double ParseFps(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return 0;

        var parts = value.Split('/');
        if (parts.Length != 2) return ParseDouble(value);

        var numerator = ParseDouble(parts[0]);
        var denominator = ParseDouble(parts[1]);

        return denominator > 0 ? numerator / denominator : 0;
    }
}
