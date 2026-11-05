using Android.App;
using Android.Content;

namespace Edutor.Nodes.App.Platforms.Android;

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

        try
        {
            NodeForegroundService.Start(context);
        }
        catch (Exception ex) when (ex is ForegroundServiceStartNotAllowedException or Java.Lang.SecurityException)
        {
            // Bəzi ROM-lar boot-da foreground xidmətini bağlayır: istifadəçi tətbiqi
            // əl ilə açanda işə düşür — başqa çıxış yolu yoxdur.
        }
    }
}
