using Edutor.Nodes.Worker;

namespace Edutor.Nodes.App;

/// <summary>
/// Worker motorunun tək nüsxəsi. UI və Android-in arxa plan xidməti eyni engine-i
/// paylaşır — iki ayrı engine eyni tapşırıqları götürməyə çalışardı.
/// </summary>
public static class WorkerHost
{
    private sealed record State(WorkerSettings Settings, WorkerLog Log, WorkerEngine Engine);

    private static readonly Lazy<State> Instance = new(Create);

    public static WorkerSettings Settings => Instance.Value.Settings;

    public static WorkerLog Log => Instance.Value.Log;

    public static WorkerEngine Engine => Instance.Value.Engine;

    /// <summary>
    /// Worker-ı qeydiyyat və "avtomatik başlat" şərtləri ödənirsə qaldırır.
    /// UI açılmayanda (telefon yenidən başladıqda) yeganə başlanğıc yoludur.
    /// </summary>
    public static bool TryAutoStart()
    {
        var state = Instance.Value;

        if (!state.Settings.IsRegistered || !state.Settings.AutoStart) return false;
        if (state.Engine.IsRunning) return true;

        _ = StartSafelyAsync(state);
        return true;
    }

    /// <summary>Arxa planda çağırılır: xəta prosesi çökdürməməlidir.</summary>
    private static async Task StartSafelyAsync(State state)
    {
        try
        {
            await state.Engine.StartAsync();
        }
        catch (Exception ex)
        {
            state.Log.Error("worker", ex.ToString());
        }
    }

    private static State Create()
    {
        var settings = WorkerSettings.Load();

        if (PlatformExtras.Current.NativeBinaryDirectory is { Length: > 0 } binaries)
        {
            settings.PlatformBinaryDirectory = binaries;
        }

        var log = new WorkerLog();
        var engine = new WorkerEngine(settings, log, PlatformExtras.Current.PowerStatus);

        return new State(settings, log, engine);
    }
}
