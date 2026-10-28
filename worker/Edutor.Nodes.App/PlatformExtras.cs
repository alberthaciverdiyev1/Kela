using Edutor.Nodes.Worker.Device;

namespace Edutor.Nodes.App;

/// <summary>
/// Platforma xas imkanlar (icazələr, batareya muafiyyəti). Masaüstündə heç nə
/// tələb olunmur; Android öz tətbiqini qeydiyyatdan keçirir.
/// </summary>
public interface IPlatformExtras
{
    bool IsSupported { get; }

    /// <summary>İstifadəçiyə göstərilən cari vəziyyət.</summary>
    string Status();

    /// <summary>Çatışmayan icazələri istəyir (sistem dialoqlarını açır).</summary>
    Task RequestAllAsync();

    /// <summary>
    /// İkililərin (ffmpeg/ffprobe) yerləşdiyi platforma qovluğu. Android-də tətbiqin
    /// native kitabxana qovluğudur — jniLibs-dən çıxan fayllar yalnız oradan icra olunur.
    /// </summary>
    string? NativeBinaryDirectory { get; }

    /// <summary>Batareya və temperatur mənbəyi. Masaüstündə null.</summary>
    IPlatformStatus? PowerStatus { get; }
}

public sealed class NoPlatformExtras : IPlatformExtras
{
    public bool IsSupported => false;

    public string Status() => string.Empty;

    public Task RequestAllAsync() => Task.CompletedTask;

    public string? NativeBinaryDirectory => null;

    public IPlatformStatus? PowerStatus => null;
}

public static class PlatformExtras
{
    public static IPlatformExtras Current { get; set; } = new NoPlatformExtras();
}
