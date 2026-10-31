using Edutor.Core;
using Edutor.Core.Common.Storage;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace Edutor.Tests.Media;

/// <summary>Storage dekore olunub — yəni skan yükləmələr optimallaşdırma yolu ilə keçir.</summary>
public class ImageUploadIntegrationTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"edutor-img-{Guid.NewGuid():N}");
    private readonly ServiceProvider _provider;

    public ImageUploadIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = "Host=localhost;Database=unused",
                ["Storage:Provider"] = "Local",
                ["Storage:RootPath"] = _root,
                ["Storage:Images:MaxDimension"] = "1920",
                ["Storage:Images:JpegQuality"] = "82",
                ["Storage:Images:MinSizeKb"] = "0"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IWebHostEnvironment>(new FakeEnvironment(_root));
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddLogging();
        services.AddCore(configuration);

        _provider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        _provider.Dispose();
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }

    [Fact]
    public async Task Upload_goes_through_optimizer_and_stores_smaller_file()
    {
        using var scope = _provider.CreateScope();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();
        Assert.IsType<ImageOptimizingFileStorage>(storage);

        using var bitmap = new SKBitmap(4000, 3000);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.CornflowerBlue);
            for (var x = 0; x < 4000; x += 20)
                canvas.DrawRect(x, 0, 8, 3000, new SKPaint { Color = SKColors.DarkOrange });
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        var original = data.ToArray();

        var stream = new MemoryStream(original);
        var file = new FormFile(stream, 0, stream.Length, "file", "banner.jpg")
        {
            Headers = new HeaderDictionary { ["Content-Type"] = "image/jpeg" },
            ContentType = "image/jpeg"
        };

        var path = await storage.UploadAsync(file, "tenant-1/banner");
        var stored = new FileInfo(Path.Combine(_root, path));

        Assert.True(stored.Exists);
        Assert.True(stored.Length < original.Length, $"{stored.Length} >= {original.Length}");

        await using var storedStream = stored.OpenRead();
        using var decoded = SKBitmap.Decode(storedStream);
        Assert.Equal(1920, Math.Max(decoded.Width, decoded.Height));
    }

    private sealed class FakeEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Edutor.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = root;
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ContentRootPath { get; set; } = root;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
