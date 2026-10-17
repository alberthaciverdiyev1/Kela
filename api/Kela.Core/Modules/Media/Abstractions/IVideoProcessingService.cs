using Kela.Core.Modules.Media.Requests;

namespace Kela.Core.Modules.Media.Abstractions;

/// <summary>Xarici video konversiya worker-ının nəticələrini qeydə alır.</summary>
public interface IVideoProcessingService
{
    /// <summary>Worker işə başlayanda videonu "processing" edir. Tapılmasa false.</summary>
    Task<bool> MarkProcessingAsync(string sourcePath, CancellationToken ct = default);

    /// <summary>Konversiya nəticəsini yazır (ready/failed). Tapılmasa false — kuyruk təkrar yoxlamalıdır.</summary>
    Task<bool> MarkProcessedAsync(VideoProcessedRequest request, CancellationToken ct = default);

    /// <summary>
    /// Nəticəni ID ilə yazır. Yol ilə axtarış qeyri-müəyyəndir: bir neçə video qeydi
    /// eyni mənbə faylına işarə edə bilər (eyni fayl bir neçə dərsdə istifadə olunur),
    /// o halda yol üzrə axtarış həmişə ən böyük ID-ni tapır və qalan videolar heç vaxt
    /// yayına çıxmır.
    /// </summary>
    Task<bool> MarkProcessedAsync(long videoId, VideoProcessedRequest request, CancellationToken ct = default);
}
