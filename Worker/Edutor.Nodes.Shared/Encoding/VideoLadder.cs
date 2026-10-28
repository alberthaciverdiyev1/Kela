namespace Edutor.Nodes.Shared.Encoding;

/// <summary>Standart HLS merdivəni: 360p/480p/720p/1080p.</summary>
public static class VideoLadder
{
    /// <summary>HLS seqment uzunluğu (saniyə).</summary>
    public const int SegmentSeconds = 8;

    public static List<EncodingProfile> Default() =>
    [
        new() { Name = "360p", Height = 360, VideoBitrateKbps = 700, AudioBitrateKbps = 96 },
        new() { Name = "480p", Height = 480, VideoBitrateKbps = 1200, AudioBitrateKbps = 128 },
        new() { Name = "720p", Height = 720, VideoBitrateKbps = 2500, AudioBitrateKbps = 128 },
        new() { Name = "1080p", Height = 1080, VideoBitrateKbps = 4500, AudioBitrateKbps = 192 }
    ];

    /// <summary>
    /// Mənbədən aşağı olan pillələri seçir. Mənbə 720p-dirsə 1080p-ə upscale etmirik,
    /// amma 720p-nin özünü saxlayırıq (yenidən kodlaşdırma keyfiyyəti qorusun).
    /// </summary>
    public static List<EncodingProfile> ForSource(IEnumerable<EncodingProfile> ladder, int? sourceHeight)
    {
        var profiles = ladder.ToList();
        if (sourceHeight is not > 0) return profiles;

        var selected = profiles.Where(p => p.Height <= sourceHeight.Value).ToList();

        // Mənbə ən aşağı pillədən də kiçikdirsə (məs. 240p) — heç olmasa bir variant çıxarılsın.
        if (selected.Count == 0) selected.Add(profiles.OrderBy(p => p.Height).First());

        return selected;
    }
}
