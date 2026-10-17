using Kela.Core.Common.Storage;
using Kela.Core.Modules.Media.Responses;
using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Media.Services;

public sealed class UploadService(IFileStorage storage) : Abstractions.IUploadService
{
    public async Task<UploadResponse> UploadAsync(IFormFile file, string folder, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("File is required");
        }

        var path = await storage.UploadAsync(file, folder, ct);
        return new UploadResponse(path, storage.GetUrl(path));
    }
}
