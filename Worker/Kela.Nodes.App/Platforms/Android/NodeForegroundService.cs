using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Kela.Nodes.Worker;

namespace Kela.Nodes.App.Platforms.Android;

/// <summary>
/// Uzun süren konversiyalar üçün foreground xidməti. Android arxa fonda işləyən
/// prosesi dayandırdığı üçün worker yalnız bu xidmət altında etibarlı işləyir.
/// Bildiriş cari tapşırığın vəziyyətini göstərir.
/// </summary>
[Service(
    Exported = false,
    ForegroundServiceType = global::Android.Content.PM.ForegroundService.TypeDataSync)]
public sealed class NodeForegroundService : Service
{
    private const string ChannelId = "kela-nodes";
    private const int NotificationId = 1001;

    private static NodeForegroundService? _current;

    /// <summary>Xidmətin göstərəcəyi mətn — UI tərəfindən yenilənir.</summary>
    public static void Publish(string title, string text)
    {
        var service = _current;
        if (service is null) return;

        service.Notify(title, text);
    }

    public static void Start(Context context)
    {
        var intent = new Intent(context, typeof(NodeForegroundService));

        if (Build.VERSION.SdkInt >= BuildVersionCodes.O) context.StartForegroundService(intent);
        else context.StartService(intent);
    }

    public static void Stop(Context context)
        => context.StopService(new Intent(context, typeof(NodeForegroundService)));

    public override void OnCreate()
    {
        base.OnCreate();
        _current = this;

        CreateChannel();
        StartForeground(NotificationId, BuildNotification("Kela Nodes", "Konversiya worker-i hazırdır"));
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        => StartCommandResult.Sticky;

    public override IBinder? OnBind(Intent? intent) => null;

    public override void OnDestroy()
    {
        _current = null;
        base.OnDestroy();
    }

    private void CreateChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;

        var manager = (NotificationManager?)GetSystemService(NotificationService);
        if (manager is null) return;

        var channel = new NotificationChannel(ChannelId, "Konversiya", NotificationImportance.Low)
        {
            Description = "Video konversiya gedişatı"
        };

        channel.SetShowBadge(false);
        manager.CreateNotificationChannel(channel);
    }

    private void Notify(string title, string text)
    {
        var manager = (NotificationManager?)GetSystemService(NotificationService);
        manager?.Notify(NotificationId, BuildNotification(title, text));
    }

    private Notification BuildNotification(string title, string text)
    {
        var openApp = PendingIntent.GetActivity(
            this, 0, new Intent(this, typeof(MainActivity)), PendingIntentFlags.Immutable);

        var builder = Build.VERSION.SdkInt >= BuildVersionCodes.O
            ? new Notification.Builder(this, ChannelId)
            : new Notification.Builder(this);

        return builder
            .SetContentTitle(title)
            .SetContentText(text)
            .SetSmallIcon(global::Android.Resource.Drawable.IcMediaPlay)
            .SetContentIntent(openApp)
            .SetOngoing(true)
            .Build()!;
    }
}

/// <summary>UI-ı xidmətə bağlayır: hər vəziyyət dəyişikliyində bildiriş yenilənir.</summary>
public static class AndroidNotificationBridge
{
    public static void Attach(WorkerEngine engine)
    {
        engine.StateChanged += () =>
        {
            var job = engine.Jobs.FirstOrDefault();

            var text = job is null
                ? (engine.IsRunning ? engine.PausedReason ?? "İş gözlənilir" : "Dayanıb")
                : $"video {job.VideoId} · {job.Resolution} · {job.Progress * 100:0}%";

            NodeForegroundService.Publish("Kela Nodes", text);
        };
    }
}
