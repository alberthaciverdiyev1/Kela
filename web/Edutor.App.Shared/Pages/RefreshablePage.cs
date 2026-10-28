using Microsoft.AspNetCore.Components;
using Edutor.App.Shared.Services;

namespace Edutor.App.Shared.Pages;

/// <summary>
/// Aşağı çəkərək yeniləməni dəstəkləyən səhifə. Səhifə öz yükləmə metodunu
/// <see cref="OnRefresh"/>-ə verir; yeniləmə zamanı səhifə yenidən qurulmur,
/// ona görə ekran skeletona düşmür — yalnız məlumat dəyişir.
/// </summary>
public abstract class RefreshablePage : ComponentBase, IDisposable
{
    [Inject] protected RefreshSignal Refresh { get; set; } = default!;

    private IDisposable? _subscription;

    protected void OnRefresh(Func<Task> handler) => _subscription = Refresh.Subscribe(async () =>
    {
        await handler();
        await InvokeAsync(StateHasChanged);
    });

    public virtual void Dispose() => _subscription?.Dispose();
}
