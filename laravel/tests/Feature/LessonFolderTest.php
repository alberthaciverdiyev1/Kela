<?php

namespace Tests\Feature;

use App\Application\Lesson\LessonService;
use App\Application\LessonFolder\LessonFolderService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class LessonFolderTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;
    protected User $admin;
    protected User $otherTeacher;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }

        $this->teacher = User::factory()->create();
        $this->teacher->assignRole(User::ROLE_TEACHER);

        $this->admin = User::factory()->create();
        $this->admin->assignRole(User::ROLE_ADMIN);

        $this->otherTeacher = User::factory()->create();
        $this->otherTeacher->assignRole(User::ROLE_TEACHER);
    }

    private function folders(): LessonFolderService
    {
        return app(LessonFolderService::class);
    }

    private function lessons(): LessonService
    {
        return app(LessonService::class);
    }

    private function makeLesson(int $teacherId, array $overrides = []): int
    {
        return $this->lessons()->create($teacherId, array_merge([
            'title' => 'Riyaziyyat Dərsi',
            'description' => 'Giriş',
        ], $overrides))->getKey();
    }

    public function test_lesson_index_page_renders_folders_and_breadcrumbs(): void
    {
        $parent = $this->folders()->createFolder($this->teacher->id, 'Riyaziyyat');
        $child = $this->folders()->createFolder($this->teacher->id, 'Alt', $parent->id);
        $this->makeLesson($this->teacher->id);

        $this->actingAs($this->teacher);

        // Kök səhifə qovluqları göstərir
        $html = $this->get('/teacher/lessons')->assertOk()->getContent();
        $this->assertStringContainsString('Riyaziyyat', $html);
        $this->assertStringContainsString('data-kind="folder"', $html);
        $this->assertStringContainsString('@contextmenu.prevent', $html);
        $this->assertStringContainsString('x-data="lessonFolders', $html);

        // List/Grid görünüm keçidi və grid kartları mövcuddur
        $this->assertStringContainsString('setViewMode(\'list\')', $html);
        $this->assertStringContainsString('setViewMode(\'grid\')', $html);
        $this->assertStringContainsString('viewMode === \'list\'', $html);
        $this->assertStringContainsString('viewMode === \'grid\'', $html);
        $this->assertStringContainsString('Liste görünümü', $html);
        $this->assertStringContainsString('Grid görünümü', $html);
        $this->assertStringContainsString('grid-cols-6', $html);

        // Alt qovluq səhifəsi breadcrumb göstərir
        $html = $this->get('/teacher/lessons?folder_id='.$child->id)->assertOk()->getContent();
        $this->assertStringContainsString('Riyaziyyat', $html);
        $this->assertStringContainsString('Alt', $html);
    }

    public function test_lesson_created_with_folder_id_and_filters_by_folder(): void
    {
        $folder = $this->folders()->createFolder($this->teacher->id, 'Fizika');
        $otherFolder = $this->folders()->createFolder($this->teacher->id, 'Kimya');
        $this->makeLesson($this->teacher->id, ['folder_id' => $folder->id]);
        $this->makeLesson($this->teacher->id, ['folder_id' => $otherFolder->id]);

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/lessons?folder_id='.$folder->id)->assertOk()->getContent();
        $this->assertStringContainsString('Fizika', $html);
        $this->assertStringContainsString('data-kind="lesson"', $html);
    }

    public function test_api_creates_lesson_folder_and_moves_lesson(): void
    {
        Sanctum::actingAs($this->teacher);

        // Qovluq yarat
        $created = $this->postJson('/api/v1/lesson-folders', ['name' => 'Tarix'])
            ->assertStatus(201)->json('data');
        $folderId = $created['id'];

        // Dərs yarat və folder-a ata
        $lesson = $this->postJson('/api/v1/lessons', [
            'title' => 'Tarix Dərsi',
            'folder_id' => $folderId,
        ])->assertStatus(201)->json('data');

        // Dərs folder-a atandı
        $this->assertSame($folderId, $lesson['folder_id']);

        // Folder dərs sayı 1-dir
        $this->assertEquals(1, $this->folders()->find($folderId)->lessons()->count());

        // Dərsi kökə daşı
        $this->postJson('/api/v1/lesson-folders/move-lesson', [
            'content_id' => $lesson['content_id'],
            'folder_id' => null,
        ])->assertOk();

        $lessonData = $this->lessons()->find($lesson['content_id']);
        $this->assertNull($lessonData->folder_id);
    }

    public function test_api_renames_and_moves_folder(): void
    {
        Sanctum::actingAs($this->teacher);

        $parent = $this->folders()->createFolder($this->teacher->id, 'Kök Qovluq');
        $child = $this->folders()->createFolder($this->teacher->id, 'Köhnə Ad');

        // Adını dəyiş
        $this->postJson("/api/v1/lesson-folders/{$child->id}/rename", ['name' => 'Yeni Ad'])
            ->assertOk();
        $this->assertEquals('Yeni Ad', $this->folders()->find($child->id)->name);

        // Parent-a daşı
        $this->postJson("/api/v1/lesson-folders/{$child->id}/move", ['parent_id' => $parent->id])
            ->assertOk();
        $this->assertEquals($parent->id, $this->folders()->find($child->id)->parent_id);
    }

    public function test_api_deletes_folder_moving_lessons_to_root(): void
    {
        Sanctum::actingAs($this->teacher);

        $folder = $this->folders()->createFolder($this->teacher->id, 'Silinəcək');
        $lessonId = $this->makeLesson($this->teacher->id, ['folder_id' => $folder->id]);

        $this->deleteJson("/api/v1/lesson-folders/{$folder->id}")->assertOk();

        // Qovluq silindi, dərs kökə düşdü
        $this->assertNull($this->folders()->find($folder->id));
        $this->assertNull($this->lessons()->find($lessonId)->folder_id);
    }

    public function test_folder_ownership_prevents_cross_teacher_access(): void
    {
        $folder = $this->folders()->createFolder($this->teacher->id, 'Şəxsi');
        $lessonId = $this->makeLesson($this->teacher->id, ['folder_id' => $folder->id]);

        Sanctum::actingAs($this->otherTeacher);

        $this->postJson("/api/v1/lesson-folders/{$folder->id}/rename", ['name' => 'X'])->assertStatus(403);
        $this->deleteJson("/api/v1/lesson-folders/{$folder->id}")->assertStatus(403);
        $this->postJson('/api/v1/lesson-folders/move-lesson', [
            'content_id' => $lessonId,
            'folder_id' => null,
        ])->assertStatus(403);
    }

    public function test_lesson_form_renders_folder_select(): void
    {
        $folder = $this->folders()->createFolder($this->teacher->id, 'Qovluq');
        $lessonId = $this->makeLesson($this->teacher->id, ['folder_id' => $folder->id]);

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/lessons/create')->assertOk()->getContent();
        $this->assertStringContainsString('name="folder_id"', $html);
        $this->assertStringContainsString('Qovluq', $html);

        $html = $this->get("/teacher/lessons/{$lessonId}/edit")->assertOk()->getContent();
        $this->assertStringContainsString('name="folder_id"', $html);
    }
}
