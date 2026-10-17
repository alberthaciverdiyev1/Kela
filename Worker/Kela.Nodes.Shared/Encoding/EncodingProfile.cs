namespace Kela.Nodes.Shared.Encoding;

/// <summary>
/// Bir HLS variantının kodlaşdırma parametrləri. Bitrate-lər <b>başlanğıc</b> dəyərlərdir —
/// konfiqurasiyadan dəyişdirilə bilər.
/// </summary>
public sealed class EncodingProfile
{
    /// <summary>Variant adı — həm də bucket-da qovluq adı (məs. "720p").</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Hədəf hündürlük. Eni aspect ratio ilə hesablanır.</summary>
    public int Height { get; set; }

    /// <summary>Video bitrate (Kbps).</summary>
    public int VideoBitrateKbps { get; set; }

    /// <summary>Audio bitrate (Kbps).</summary>
    public int AudioBitrateKbps { get; set; } = 128;

    /// <summary>VBV maksimum bitrate (Kbps) — 0 = hesablanır (video bitrate × 1.5).</summary>
    public int MaxRateKbps { get; set; }

    /// <summary>VBV buffer size (Kbps) — 0 = hesablanır (video bitrate × 2).</summary>
    public int BufSizeKbps { get; set; }

    public int EffectiveMaxRateKbps => MaxRateKbps > 0 ? MaxRateKbps : (int)(VideoBitrateKbps * 1.5);

    public int EffectiveBufSizeKbps => BufSizeKbps > 0 ? BufSizeKbps : VideoBitrateKbps * 2;

    public EncodingProfile Clone() => new()
    {
        Name = Name,
        Height = Height,
        VideoBitrateKbps = VideoBitrateKbps,
        AudioBitrateKbps = AudioBitrateKbps,
        MaxRateKbps = MaxRateKbps,
        BufSizeKbps = BufSizeKbps
    };
}
