using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace Edutor.Core.Common.Storage;

public sealed class ImageOptimizerOptions
{
    public const string SectionName = "Storage:Images";

    public bool Enabled { get; set; } = true;
    public int MaxDimension { get; set; } = 1920;
    public int JpegQuality { get; set; } = 82;
    public int MinSizeKb { get; set; } = 24;
}

/// <summary>
/// Yüklənən şəkilləri yenidən kodlayır: böyük ölçüləri kiçildir, JPEG keyfiyyətini
/// azaldır, EXIF oriyentasiyasını tətbiq edir. Nəticə orijinaldan böyük olarsa
/// orijinal saxlanılır.
/// </summary>
public sealed class ImageOptimizer(IConfiguration configuration, ILogger<ImageOptimizer> logger)
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".bmp" };

    public ImageOptimizerOptions Options { get; } = Bind(configuration);

    private static ImageOptimizerOptions Bind(IConfiguration configuration)
    {
        var options = new ImageOptimizerOptions();
        configuration.GetSection(ImageOptimizerOptions.SectionName).Bind(options);
        return options;
    }

    public bool CanOptimize(IFormFile file)
    {
        if (!Options.Enabled || file.Length == 0 || file.Length < Options.MinSizeKb * 1024L) return false;

        var extension = Path.GetExtension(file.FileName);
        if (!SupportedExtensions.Contains(extension)) return false;

        // SVG/GIF istisna: SkiaSharp onları rasterləşdirmir.
        return file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<IFormFile> OptimizeAsync(IFormFile file, CancellationToken ct = default)
    {
        if (!CanOptimize(file)) return file;

        try
        {
            byte[] source;
            await using (var input = file.OpenReadStream())
            using (var buffer = new MemoryStream())
            {
                await input.CopyToAsync(buffer, ct);
                source = buffer.ToArray();
            }

            var result = await Task.Run(() => Reencode(source, Path.GetExtension(file.FileName)), ct);
            if (result is null || result.Value.Data.Length >= source.Length) return file;

            var extension = result.Value.Extension;
            var name = Path.GetFileNameWithoutExtension(file.FileName) + extension;
            var stream = new MemoryStream(result.Value.Data);

            return new FormFile(stream, 0, stream.Length, file.Name, name)
            {
                Headers = new HeaderDictionary { ["Content-Type"] = result.Value.ContentType },
                ContentType = result.Value.ContentType
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Şəkil korlanıbsa yükləmə yenə də davam etsin; optimallaşdırma köməkçi addımdır.
            logger.LogWarning(ex, "Image optimization failed for {FileName}, original kept", file.FileName);
            return file;
        }
    }

    private (byte[] Data, string Extension, string ContentType)? Reencode(byte[] source, string extension)
    {
        using var codec = SKCodec.Create(new SKMemoryStream(source));
        if (codec is null) return null;

        var info = codec.Info;
        using var bitmap = new SKBitmap(info.Width, info.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
        var decodeResult = codec.GetPixels(bitmap.Info, bitmap.GetPixels());
        if (decodeResult is not (SKCodecResult.Success or SKCodecResult.IncompleteInput)) return null;

        using var oriented = ApplyOrigin(bitmap, codec.EncodedOrigin);
        using var resized = Resize(oriented, Options.MaxDimension);

        // Şəffaflıq PNG/WebP-də saxlanılır; JPEG-ə çevirmirik ki, fon qaralmasın.
        var hasAlpha = resized.AlphaType != SKAlphaType.Opaque;
        var format = extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && hasAlpha
            ? (Extension: ".png", ContentType: "image/png", Encoded: SKEncodedImageFormat.Png)
            : (Extension: ".jpg", ContentType: "image/jpeg", Encoded: SKEncodedImageFormat.Jpeg);

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(format.Encoded, Options.JpegQuality);
        if (data is null) return null;

        return (data.ToArray(), format.Extension, format.ContentType);
    }

    private static SKBitmap ApplyOrigin(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        if (origin is SKEncodedOrigin.Default or SKEncodedOrigin.TopLeft) return bitmap.Copy();

        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;

        var width = swap ? bitmap.Height : bitmap.Width;
        var height = swap ? bitmap.Width : bitmap.Height;

        var rotated = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(rotated))
        {
            canvas.Translate(width / 2f, height / 2f);
            switch (origin)
            {
                case SKEncodedOrigin.TopRight: canvas.Scale(-1, 1); break;
                case SKEncodedOrigin.BottomRight: canvas.Scale(-1, -1); break;
                case SKEncodedOrigin.BottomLeft: canvas.Scale(1, -1); break;
                case SKEncodedOrigin.LeftTop: canvas.RotateDegrees(90); canvas.Scale(1, -1); break;
                case SKEncodedOrigin.RightTop: canvas.RotateDegrees(90); break;
                case SKEncodedOrigin.RightBottom: canvas.RotateDegrees(-90); canvas.Scale(1, -1); break;
                case SKEncodedOrigin.LeftBottom: canvas.RotateDegrees(-90); break;
            }

            canvas.Translate(-bitmap.Width / 2f, -bitmap.Height / 2f);
            canvas.DrawBitmap(bitmap, 0, 0, new SKSamplingOptions(SKFilterMode.Nearest));
        }

        return rotated;
    }

    private static SKBitmap Resize(SKBitmap bitmap, int maxDimension)
    {
        var longest = Math.Max(bitmap.Width, bitmap.Height);
        if (longest <= maxDimension) return bitmap.Copy();

        var scale = (float)maxDimension / longest;
        var width = Math.Max(1, (int)Math.Round(bitmap.Width * scale));
        var height = Math.Max(1, (int)Math.Round(bitmap.Height * scale));

        var resized = bitmap.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        return resized ?? bitmap.Copy();
    }
}
