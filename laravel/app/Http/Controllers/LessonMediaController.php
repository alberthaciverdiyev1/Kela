<?php

namespace App\Http\Controllers;

use App\Models\Lesson;
use App\Services\MediaProcessor;
use Symfony\Component\HttpFoundation\Response;
use Symfony\Component\HttpFoundation\StreamedResponse;

/**
 * Ders videolarını (Range destekli) ve thumbnail'lerini yayınlar.
 * .NET tarafındaki /api/lessons/{id}/stream ve static /uploads karşılığıdır.
 */
class LessonMediaController extends Controller
{
    public function __construct(private readonly MediaProcessor $media)
    {
    }

    /** Video akışı — Range destekli, öğrenciler yayınlanmış dersleri izleyebilir. */
    public function stream(int $contentId): Response
    {
        $lesson = Lesson::with('content')->where('content_id', $contentId)->first();
        if (! $lesson || empty($lesson->video_path) || ! $this->canView($lesson)) {
            abort(404);
        }

        $full = $this->media->resolveFullPath($lesson->video_path);
        if ($full === null) {
            abort(404);
        }

        return $this->streamWithRange($full);
    }

    /** Thumbnail yayınlar. */
    public function thumbnail(int $contentId): Response
    {
        $lesson = Lesson::with('content')->where('content_id', $contentId)->first();
        if (! $lesson || empty($lesson->thumbnail_path) || ! $this->canView($lesson)) {
            abort(404);
        }

        $full = $this->media->resolveFullPath($lesson->thumbnail_path);
        if ($full === null) {
            abort(404);
        }

        return response()->file($full, [
            'Content-Type' => MediaProcessor::contentTypeFor($full),
            'Cache-Control' => 'private, max-age=3600',
        ]);
    }

    private function canView(Lesson $lesson): bool
    {
        $user = auth()->user();
        if (! $user) {
            return false;
        }

        // Admin her zaman görebilir.
        if ($user->isAdmin()) {
            return true;
        }

        // Sahip öğretmen görebilir.
        if ($user->isTeacher()) {
            return $lesson->teacher_id === $user->id;
        }

        // Öğrenci yalnızca yayınlanmış dersleri izleyebilir.
        if ($user->isStudent() || $user->isParent()) {
            return (bool) ($lesson->content?->is_published);
        }

        return false;
    }

    /** Range isteklerini destekleyen basit video akışı. */
    private function streamWithRange(string $full): Response
    {
        $size = filesize($full);
        $mime = MediaProcessor::contentTypeFor($full);
        $range = request()->header('Range');

        if ($range && preg_match('/bytes=(\d*)-(\d*)/', $range, $m)) {
            $start = $m[1] !== '' ? (int) $m[1] : 0;
            $end = $m[2] !== '' ? (int) $m[2] : $size - 1;

            if ($start > $end || $start >= $size) {
                return response('', 416)->header('Content-Range', "bytes */{$size}");
            }

            $end = min($end, $size - 1);
            $length = $end - $start + 1;

            return response()->stream(
                function () use ($full, $start, $length): void {
                    $fp = fopen($full, 'rb');
                    if ($fp === false) {
                        return;
                    }
                    fseek($fp, $start);
                    echo fread($fp, $length);
                    fclose($fp);
                },
                206,
                [
                    'Content-Type' => $mime,
                    'Content-Length' => (string) $length,
                    'Accept-Ranges' => 'bytes',
                    'Content-Range' => "bytes {$start}-{$end}/{$size}",
                    'Cache-Control' => 'private, max-age=3600',
                ],
            );
        }

        return response()->file($full, [
            'Content-Type' => $mime,
            'Accept-Ranges' => 'bytes',
            'Cache-Control' => 'private, max-age=3600',
        ]);
    }
}
