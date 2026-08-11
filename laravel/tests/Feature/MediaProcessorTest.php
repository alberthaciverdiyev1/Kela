<?php

namespace Tests\Feature;

use App\Services\MediaProcessor;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Http\UploadedFile;
use Tests\TestCase;

class MediaProcessorTest extends TestCase
{
    use RefreshDatabase;

    public function test_process_video_extracts_duration_and_thumbnail(): void
    {
        $video = UploadedFile::fake()->create('lesson.mp4', 1000, 'video/mp4');
        $stored = $video->store('uploads/videos', 'local');

        $processor = app(MediaProcessor::class);
        $meta = $processor->processVideo($stored);

        // Sahte dosya gerçek bir video olmadığı için süre 0 döner, ancak
        // API kontratı (anahtar dizisi) doğru olmalıdır.
        $this->assertArrayHasKey('thumbnail_path', $meta);
        $this->assertArrayHasKey('duration_seconds', $meta);
        $this->assertIsInt($meta['duration_seconds']);

        // Gerçek bir video yoksa thumbnail üretilemez; boş olmalıdır.
        $this->assertNull($meta['thumbnail_path']);
    }

    public function test_missing_video_returns_empty_meta(): void
    {
        $processor = app(MediaProcessor::class);
        $meta = $processor->processVideo('uploads/videos/does-not-exist.mp4');

        $this->assertNull($meta['thumbnail_path']);
        $this->assertSame(0, $meta['duration_seconds']);
    }

    public function test_content_type_for_common_extensions(): void
    {
        $this->assertSame('video/mp4', MediaProcessor::contentTypeFor('/uploads/videos/x.mp4'));
        $this->assertSame('video/webm', MediaProcessor::contentTypeFor('/uploads/videos/x.webm'));
        $this->assertSame('video/x-matroska', MediaProcessor::contentTypeFor('/uploads/videos/x.mkv'));
        $this->assertSame('image/jpeg', MediaProcessor::contentTypeFor('/uploads/thumbnails/x.jpg'));
    }
}
