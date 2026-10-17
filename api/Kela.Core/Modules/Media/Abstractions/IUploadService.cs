using Kela.Core.Modules.Media.Responses;
using Microsoft.AspNetCore.Http;

namespace Kela.Core.Modules.Media.Abstractions;

public interface IUploadService
{
    Task<UploadResponse> UploadAsync(IFormFile file, string folder, CancellationToken ct = default);
}
