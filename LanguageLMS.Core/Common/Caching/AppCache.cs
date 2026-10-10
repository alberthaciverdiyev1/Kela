using Microsoft.Extensions.Caching.Memory;

namespace LanguageLMS.Core.Common.Caching;

public interface IAppCache
{
    void Clear();
}

/// <summary>Memory cache-i tam təmizləmək üçün (IMemoryCache Clear dəstəkləmir).</summary>
public sealed class AppCache : IAppCache, IDisposable
{
    private readonly Lock _lock = new();
    private MemoryCache _cache = new(new MemoryCacheOptions());

    public IMemoryCache Cache => _cache;

    public void Clear()
    {
        lock (_lock)
        {
            var old = _cache;
            _cache = new MemoryCache(new MemoryCacheOptions());
            old.Dispose();
        }
    }

    public void Dispose() => _cache.Dispose();
}
