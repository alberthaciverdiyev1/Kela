namespace Kela.App.Shared.Services;

/// <summary>
/// Aşağı çəkərək yeniləmə siqnalı. Səhifələr öz yükləmə metodunu abunə edir,
/// MainLayout isə siqnalı qaldırır. Səhifə yenidən qurulmur — beləcə ekran
/// skeletona düşmür, mövcud məzmun yerində qalır.
/// </summary>
public sealed class RefreshSignal
{
    private readonly List<Func<Task>> _handlers = [];

    public IDisposable Subscribe(Func<Task> handler)
    {
        _handlers.Add(handler);
        return new Subscription(() => _handlers.Remove(handler));
    }

    public async Task RaiseAsync()
    {
        // Kopya üzərində gedirik: handler özünü abunəlikdən çıxara bilər.
        foreach (var handler in _handlers.ToArray())
        {
            try
            {
                await handler();
            }
            catch
            {
                /* bir səhifənin xətası digərini saxlamasın */
            }
        }
    }

    private sealed class Subscription(Action release) : IDisposable
    {
        public void Dispose() => release();
    }
}
