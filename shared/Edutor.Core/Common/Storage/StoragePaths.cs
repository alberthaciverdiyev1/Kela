using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Edutor.Core.Common.Storage;

public static class StoragePaths
{
    public const string RequestPath = "/uploads";

    public static string ResolveRoot(IWebHostEnvironment env, IConfiguration config)
    {
        var configured = config["Storage:RootPath"];
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return Path.IsPathRooted(configured)
                ? configured
                : Path.GetFullPath(Path.Combine(env.ContentRootPath, configured));
        }

        return Path.Combine(env.WebRootPath, "uploads");
    }
}
