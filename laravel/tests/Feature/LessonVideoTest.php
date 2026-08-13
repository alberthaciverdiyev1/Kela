<?php

namespace Tests\Feature;

use App\Application\Lesson\LessonService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Http\UploadedFile;
use Illuminate\Support\Facades\Storage;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class LessonVideoTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }

        $this->teacher = User::factory()->create();
        $this->teacher->assignRole(User::ROLE_TEACHER);
    }

    public function test_video_upload_lands_on_local_disk_and_plays(): void
    {
        Storage::fake('local');

        $real = storage_path('app/uploads/videos/test-lesson.mp4');
        if (! is_file($real)) {
            $this->markTestSkipped('test-lesson.mp4 yoxdur.');
        }
        $video = new UploadedFile($real, 'ders.mp4', 'video/mp4', null, true);

        $this->actingAs($this->teacher);

        $this->post('/teacher/lessons', [
            'title' => 'Video dərs',
            'description' => 'Video ilə',
            'is_published' => '1',
            'order_index' => '0',
            'video' => $video,
        ])->assertRedirect(route('teacher.lessons.index'));

        $created = app(LessonService::class)->paginate($this->teacher->id)->first();
        $model = app(LessonService::class)->find($created['content_id']);
        $this->assertNotNull($model->video_path, 'video_path DB-yə yazılmalıdır');
        $this->assertTrue($model->has_video, 'has_video true olmalıdır');

        // Dosya local disk kökünde (storage/app/private) durmalı.
        $this->assertFileExists(Storage::disk('local')->path($model->video_path));

        // Show sayfası <video> içermeli ve stream URL'i verilmeli.
        $html = $this->get("/teacher/lessons/{$model->content_id}")->assertOk()->getContent();
        $this->assertStringContainsString('<video', $html);
        $this->assertStringContainsString("/lesson/{$model->content_id}/stream", $html);

        // Range'li istek 206 dönmeli.
        $stream = $this->withHeader('Range', 'bytes=0-')
            ->get("/lesson/{$model->content_id}/stream")
            ->assertStatus(206);
        $this->assertSame('bytes', $stream->headers->get('Accept-Ranges'));
        $this->assertStringStartsWith('bytes ', (string) $stream->headers->get('Content-Range'));
    }

    public function test_legacy_file_under_app_root_still_resolves(): void
    {
        // Köhnə yazım: storage/app/uploads/videos/test-lesson.mp4 (fiziki fayl mevcut).
        if (! is_file(storage_path('app/uploads/videos/test-lesson.mp4'))) {
            $this->markTestSkipped('test-lesson.mp4 yoxdur.');
        }

        $lesson = app(LessonService::class)->create($this->teacher->id, [
            'title' => 'Stream testi',
            'description' => null,
            'video_path' => 'uploads/videos/test-lesson.mp4',
            'is_published' => true,
        ]);

        $this->actingAs($this->teacher);

        $this->withHeader('Range', 'bytes=0-')
            ->get("/lesson/{$lesson->content_id}/stream")
            ->assertStatus(206);
    }
}
