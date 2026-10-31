using Edutor.Core.Common.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;

namespace Edutor.Tests.Media;

public class ImageOptimizerTests
{
    private static ImageOptimizer Create(int maxDimension = 1920, int minSizeKb = 0)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:Images:Enabled"] = "true",
                ["Storage:Images:MaxDimension"] = maxDimension.ToString(),
                ["Storage:Images:JpegQuality"] = "82",
                ["Storage:Images:MinSizeKb"] = minSizeKb.ToString()
            })
            .Build();

        return new ImageOptimizer(configuration, NullLogger<ImageOptimizer>.Instance);
    }

    private static byte[] MakeImage(int width, int height, SKEncodedImageFormat format, bool noisy = true)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(new SKColor(200, 120, 40));
            if (noisy)
            {
                var random = new Random(42);
                for (var i = 0; i < 8000; i++)
                {
                    using var paint = new SKPaint { Color = new SKColor((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)) };
                    canvas.DrawRect(random.Next(width), random.Next(height), 6, 6, paint);
                }
            }
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 100);
        return data.ToArray();
    }

    private static IFormFile AsFile(byte[] bytes, string name, string contentType)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, stream.Length, "file", name)
        {
            Headers = new HeaderDictionary { ["Content-Type"] = contentType },
            ContentType = contentType
        };
    }

    [Fact]
    public async Task Downsizes_and_shrinks_large_jpeg()
    {
        var bytes = MakeImage(4000, 3000, SKEncodedImageFormat.Jpeg);
        var file = AsFile(bytes, "photo.JPG", "image/jpeg");

        var optimized = await Create().OptimizeAsync(file);

        using var input = optimized.OpenReadStream();
        using var result = SKBitmap.Decode(input);

        Assert.Equal(1920, Math.Max(result.Width, result.Height));
        Assert.True(optimized.Length < bytes.Length);
        Assert.Equal("image/jpeg", optimized.ContentType);
        Assert.EndsWith(".jpg", optimized.FileName);
    }

    [Fact]
    public async Task Keeps_transparency_by_staying_png()
    {
        using var bitmap = new SKBitmap(3000, 2000);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);
            using var paint = new SKPaint { Color = SKColors.Red };
            canvas.DrawCircle(1500, 1000, 800, paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var file = AsFile(data.ToArray(), "logo.png", "image/png");

        var optimized = await Create().OptimizeAsync(file);

        Assert.Equal("image/png", optimized.ContentType);
        Assert.EndsWith(".png", optimized.FileName);
    }

    [Fact]
    public async Task Leaves_small_and_non_image_files_untouched()
    {
        var small = AsFile(MakeImage(64, 64, SKEncodedImageFormat.Jpeg), "tiny.jpg", "image/jpeg");
        var pdf = AsFile([1, 2, 3, 4], "doc.pdf", "application/pdf");

        var optimizer = Create(minSizeKb: 100);
        Assert.Same(small, await optimizer.OptimizeAsync(small));
        Assert.Same(pdf, await optimizer.OptimizeAsync(pdf));
    }

    [Fact]
    public async Task Ignores_svg()
    {
        var svg = AsFile("<svg/>"u8.ToArray(), "icon.svg", "image/svg+xml");
        Assert.Same(svg, await Create().OptimizeAsync(svg));
    }
}
