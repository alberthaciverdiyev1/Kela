using System.IO.Pipes;

namespace Edutor.Nodes.App.Platforms.Desktop;

/// <summary>
/// İkinci nüsxə işə düşməsin: artıq açıq olan pəncərə önə çıxarılır. Əks halda
/// sistem açılışında başlayan nüsxə ilə əl ilə açılan nüsxə eyni işi iki dəfə aparardı.
/// </summary>
public static class SingleInstance
{
    private const string PipeName = "Edutor.Nodes.Activate";

    public static bool TryActivateExisting()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(400);
                using var writer = new StreamWriter(client);
                writer.Write("show");
                writer.Flush();
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Server yenidən qurulur ola bilər — qısa gözləyib təkrar yoxlanılır.
                Thread.Sleep(150);
            }
        }

        return false;
    }

    public static void Listen(Action onActivate)
    {
        var thread = new Thread(() =>
        {
            while (true)
            {
                try
                {
                    using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1);
                    server.WaitForConnection();

                    using var reader = new StreamReader(server);
                    _ = reader.ReadToEnd();

                    onActivate();
                }
                catch (IOException)
                {
                    // Bağlantı yarımçıq qaldı — dinləmə davam edir.
                }
                catch (UnauthorizedAccessException)
                {
                    return;
                }
            }
        })
        {
            IsBackground = true,
            Name = "edutor-single-instance"
        };

        thread.Start();
    }
}
