using Android.App;
using Android.Content;

namespace Kela.Nodes.App.Platforms.Android;

/// <summary>
/// Telefon yenidən başladıqdan sonra worker xidmətini geri qaldırır — istifadəçi
/// tətbiqi açmağı unutsa da konversiya davam edir.
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false, DirectBootAware = true)]
[IntentFilter([
    Intent.ActionBootCompleted,
    Intent.ActionLockedBootCompleted,
    Intent.ActionMyPackageReplaced
])]
public sealed class BootCompletedReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null) return;

        NodeForegroundService.Start(context);
    }
}
