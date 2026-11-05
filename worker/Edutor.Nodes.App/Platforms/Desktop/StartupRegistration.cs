using System.Diagnostics;
using System.Text;

namespace Edutor.Nodes.App.Platforms.Desktop;

/// <summary>
/// Tətbiqin sistem açılışında arxa planda başlamasını qeyd edir. Hər platformanın
/// öz mexanizmi var: Linux XDG autostart, macOS LaunchAgent, Windows Run açarı.
/// </summary>
public static class StartupRegistration
{
    public static bool IsSupported
        => OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsWindows();

    public static void Apply(bool enabled)
    {
        if (!IsSupported) return;

#if DEBUG
        // İnkişaf quruluşu sistemə yazılmır — debug binary yolunu işə salardı.
        return;
#else
        try
        {
            if (OperatingSystem.IsLinux()) ApplyLinux(enabled);
            else if (OperatingSystem.IsMacOS()) ApplyMac(enabled);
            else ApplyWindows(enabled);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            // Avtomatik başlatma alınmasa tətbiq əl ilə açılır — qəbir xəta deyil.
        }
#endif
    }

    private static string ExecutablePath
        => Environment.ProcessPath
           ?? Process.GetCurrentProcess().MainModule?.FileName
           ?? string.Empty;

    // ---------------------------------------------------------------- linux

    private static string LinuxAutostartPath()
    {
        // .NET 10 XDG_CONFIG_HOME təyin olunmayanda ApplicationData üçün boş sətir
        // qaytarır; o halda fayl işçi qovluğuna düşərdi.
        var config = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

        if (string.IsNullOrEmpty(config))
        {
            config = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
        }

        return Path.Combine(config, "autostart", "edutor-nodes.desktop");
    }

    private static void ApplyLinux(bool enabled)
    {
        var path = LinuxAutostartPath();

        if (!enabled)
        {
            if (File.Exists(path)) File.Delete(path);
            return;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        var content = new StringBuilder()
            .AppendLine("[Desktop Entry]")
            .AppendLine("Type=Application")
            .AppendLine("Name=Edutor Nodes")
            .AppendLine("Comment=Video konversiya worker-i")
            .AppendLine($"Exec=\"{ExecutablePath}\" --minimized")
            .AppendLine("Icon=edutor-nodes")
            .AppendLine("Terminal=false")
            .AppendLine("NoDisplay=true")
            .AppendLine("X-GNOME-Autostart-enabled=true")
            .ToString();

        File.WriteAllText(path, content);
    }

    // ---------------------------------------------------------------- macOS

    private static string MacAgentPath()
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Library",
            "LaunchAgents",
            "com.edutor.nodes.plist");

    private static void ApplyMac(bool enabled)
    {
        var path = MacAgentPath();

        if (!enabled)
        {
            if (File.Exists(path)) File.Delete(path);
            return;
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

        // KeepAlive yoxdur: istifadəçi treydən "Çıxış" edəndə tətbiq geri qalxmamalıdır.
        var content = $"""
            <?xml version="1.0" encoding="UTF-8"?>
            <!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
            <plist version="1.0">
            <dict>
                <key>Label</key>            <string>com.edutor.nodes</string>
                <key>ProgramArguments</key>
                <array>
                    <string>{ExecutablePath}</string>
                    <string>--minimized</string>
                </array>
                <key>RunAtLoad</key>        <true/>
                <key>ProcessType</key>      <string>Interactive</string>
            </dict>
            </plist>
            """;

        File.WriteAllText(path, content);
    }

    // ---------------------------------------------------------------- windows

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ApplyWindows(bool enabled)
    {
        const string key = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string value = "Edutor Nodes";

        using var run = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(key, writable: true);

        if (run is null) return;

        if (enabled) run.SetValue(value, $"\"{ExecutablePath}\" --minimized");
        else run.DeleteValue(value, throwOnMissingValue: false);
    }
}
