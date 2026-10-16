using Kela.Core.Modules.Media.Requests;

namespace Kela.Core.Modules.Media.Abstractions;

/// <summary>Xarici video konversiya worker-ının nəticələrini qeydə alır.</summary>
public interface IVideoProcessingService
{
    /// <summary>Worker işə başlayanda videonu "processing" edir. Tapılmasa false.</summary>
    Task<bool> MarkProcessingAsync(string sourcePath, CancellationToken ct = default);

    /// <summary>Konversiya nəticəsini yazır (ready/failed). Tapılmasa false — kuyruk təkrar yoxlamalıdır.</summary>
    Task<bool> MarkProcessedAsync(VideoProcessedRequest request, CancellationToken ct = default);
}
