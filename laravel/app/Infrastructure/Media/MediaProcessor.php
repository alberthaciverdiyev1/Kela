<?php

namespace App\Infrastructure\Media;

use Illuminate\Support\Facades\Storage;
use Illuminate\Support\Str;
use Symfony\Component\Process\Process;

/**
 * Video dosyalarını diske kaydeder, ffprobe ile süreyi okur ve
 * ffmpeg ile videodan otomatik thumbnail (1. saniyedeki kare) çıkarır.
 * .NET tarafındaki Kela.Api.Media.MediaProcessor ile aynı davranışı sürdürür.
 */
class MediaProcessor
{
    public const int MAX_VIDEO_MB = 512;

    private const array VIDEO_EXTENSIONS = [
        'mp4', 'webm', 'ogg', 'mov', 'm4v', 'mkv', 'avi', 'mpg', 'mpeg',
    ];

    /** Depolama köküne göre disk dizinleri (local disk = storage/app). */
    public const string VIDEOS_DIR = 'uploads/videos';
    public const string THUMBNAILS_DIR = 'uploads/thumbnails';

    /**
     * Zaten diske kaydedilmiş bir videodan süre ve thumbnail üretir.
     *
     * @return array{thumbnail_path: string|null, duration_seconds: int}
     */
    public function processVideo(string $storedVideoPath): array
    {
        $full = $this->resolveFullPath($storedVideoPath);
        if ($full === null) {
            return ['thumbnail_path' => null, 'duration_seconds' => 0];
        }

        $duration = $this->probeDuration($full);
        $thumbnail = $this->extractThumbnail($full);

        return [
            'thumbnail_path' => $thumbnail,
            'duration_seconds' => $duration,
        ];
    }

    /**
     * DB'deki relative yol → diskteki fiziksel yol.
     *
     * Upload'lar `->store(..., 'local')` ile local diskə yazılır
     * (kök = storage/app/private). Köhnə yazımlar storage/app-in
     * özündə olduğundan, ardıcıllıq üçün əvvəl local diski yoxlayır,
     * tapılmazsa köhnə yola (legacy) da baxır.
     */
    public function resolveFullPath(string $storedPath): ?string
    {
        $path = str_starts_with($storedPath, '/') ? ltrim($storedPath, '/') : $storedPath;

        $candidates = [
            Storage::disk('local')->path($path),   // storage/app/private/uploads/videos/...
            storage_path('app/' . $path),          // legacy: storage/app/uploads/videos/...
        ];

        foreach ($candidates as $candidate) {
            if (is_file($candidate)) {
                return $candidate;
            }
        }

        return null;
    }

    /** ffprobe ile süre (saniye, yuvarlanır). */
    public function probeDuration(string $videoFile): int
    {
        $process = new Process([
            'ffprobe', '-v', 'error',
            '-show_entries', 'format=duration',
            '-of', 'csv=p=0',
            $videoFile,
        ]);
        $process->setTimeout(30);
        $process->run();

        $output = trim($process->getOutput());
        if ($output === '' || ! is_numeric($output)) {
            return 0;
        }

        return (int) round((float) $output);
    }

    /** ffmpeg ile videonun 1. saniyesindeki kareyi thumbnail olarak kaydeder. */
    public function extractThumbnail(string $videoFile): ?string
    {
        $dir = Storage::disk('local')->path(self::THUMBNAILS_DIR);
        if (! is_dir($dir) && ! mkdir($dir, 0777, true) && ! is_dir($dir)) {
            return null;
        }

        $name = Str::uuid()->toString() . '.jpg';
        $thumbFile = $dir . '/' . $name;

        $process = new Process([
            'ffmpeg', '-y',
            '-ss', '1',
            '-i', $videoFile,
            '-frames:v', '1',
            '-q:v', '2',
            $thumbFile,
        ]);
        $process->setTimeout(60);
        $process->run();

        if (! $process->isSuccessful() || ! is_file($thumbFile)) {
            return null;
        }

        return self::THUMBNAILS_DIR . '/' . $name;
    }

    public static function contentTypeFor(string $path): string
    {
        return match (strtolower(pathinfo($path, PATHINFO_EXTENSION))) {
            'mp4' => 'video/mp4',
            'webm' => 'video/webm',
            'ogg', 'ogv' => 'video/ogg',
            'mov' => 'video/quicktime',
            'm4v' => 'video/x-m4v',
            'mkv' => 'video/x-matroska',
            'avi' => 'video/x-msvideo',
            'jpg', 'jpeg' => 'image/jpeg',
            default => 'application/octet-stream',
        };
    }
}
