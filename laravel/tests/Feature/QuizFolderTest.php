<?php

namespace Tests\Feature;

use App\Application\Quiz\QuizService;
use App\Application\QuizFolder\QuizFolderService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class QuizFolderTest extends TestCase
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

    private function folders(): QuizFolderService
    {
        return app(QuizFolderService::class);
    }

    private function quizzes(): QuizService
    {
        return app(QuizService::class);
    }

    private function makeQuiz(int $teacherId, array $overrides = []): int
    {
        return $this->quizzes()->create($teacherId, array_merge([
            'title' => 'Sınaq',
            'description' => 'Fənn testi',
        ], $overrides))->getKey();
    }

    public function test_quiz_index_page_renders_folders_and_breadcrumbs(): void
    {
        $parent = $this->folders()->createFolder($this->teacher->id, 'Riyaziyyat');
        $child = $this->folders()->createFolder($this->teacher->id, 'Alt', $parent->id);
        $this->makeQuiz($this->teacher->id);

        $this->actingAs($this->teacher);

        // Kök səhifə qovluqları göstərir
        $html = $this->get('/teacher/quizzes')->assertOk()->getContent();
        $this->assertStringContainsString('Riyaziyyat', $html);
        $this->assertStringContainsString('data-folder-action', $html);
        $this->assertStringContainsString('x-data="quizFolders', $html);

        // List/Grid görünüm keçidi və grid kartları mövcuddur
        $this->assertStringContainsString('setViewMode(\'list\')', $html);
        $this->assertStringContainsString('setViewMode(\'grid\')', $html);
        $this->assertStringContainsString('viewMode === \'list\'', $html);
        $this->assertStringContainsString('viewMode === \'grid\'', $html);
        $this->assertStringContainsString('Liste görünümü', $html);
        $this->assertStringContainsString('Grid görünümü', $html);
        $this->assertStringContainsString('grid-cols-6', $html);

        // Alt qovluq səhifəsi breadcrumb göstərir
        $html = $this->get('/teacher/quizzes?folder_id='.$child->id)->assertOk()->getContent();
        $this->assertStringContainsString('Riyaziyyat', $html);
        $this->assertStringContainsString('Alt', $html);
    }

    public function test_quiz_created_with_folder_id_and_filters_by_folder(): void
    {
        $folder = $this->folders()->createFolder($this->teacher->id, 'Fizika');
        $otherFolder = $this->folders()->createFolder($this->teacher->id, 'Kimya');
        $this->makeQuiz($this->teacher->id, ['folder_id' => $folder->id]);
        $this->makeQuiz($this->teacher->id, ['folder_id' => $otherFolder->id]);

        $this->actingAs($this->teacher);

        $page = $this->get('/teacher/quizzes?folder_id='.$folder->id)->assertOk();
        $html = $page->getContent();
        $this->assertStringContainsString('Fizika', $html);

        // Foldera aid quiz görünür, digər folder quizi görünmür
        $this->assertStringContainsString('data-quiz-action', $html);
    }

    public function test_api_creates_quiz_folder_and_moves_quiz(): void
    {
        Sanctum::actingAs($this->teacher);

        // Qovluq yarat
        $created = $this->postJson('/api/v1/quiz-folders', ['name' => 'Tarix'])
            ->assertStatus(201)->json('data');
        $folderId = $created['id'];

        // Quiz yarat və folder-a ata
        $quiz = $this->postJson('/api/v1/quizzes', [
            'title' => 'Sınaq',
            'folder_id' => $folderId,
        ])->assertStatus(201)->json('data');

        // Quiz folder-a atandı
        $quizData = $this->quizzes()->formData($quiz['content_id']);
        $this->assertEquals($folderId, $quizData['folder_id']);

        // Folder qovluq sayı 1-dir
        $this->assertEquals(1, $this->folders()->find($folderId)->quizzes()->count());

        // Quiz kökə daşı
        $this->postJson('/api/v1/quiz-folders/move-quiz', [
            'content_id' => $quiz['content_id'],
            'folder_id' => null,
        ])->assertOk();

        $quizData = $this->quizzes()->formData($quiz['content_id']);
        $this->assertNull($quizData['folder_id']);
    }

    public function test_api_renames_and_moves_folder(): void
    {
        Sanctum::actingAs($this->teacher);

        $parent = $this->folders()->createFolder($this->teacher->id, 'Kök Qovluq');
        $child = $this->folders()->createFolder($this->teacher->id, 'Köhnə Ad');

        // Adını dəyiş
        $this->postJson("/api/v1/quiz-folders/{$child->id}/rename", ['name' => 'Yeni Ad'])
            ->assertOk();
        $this->assertEquals('Yeni Ad', $this->folders()->find($child->id)->name);

        // Parent-a daşı
        $this->postJson("/api/v1/quiz-folders/{$child->id}/move", ['parent_id' => $parent->id])
            ->assertOk();
        $this->assertEquals($parent->id, $this->folders()->find($child->id)->parent_id);
    }

    public function test_api_deletes_folder_moving_quizzes_to_root(): void
    {
        Sanctum::actingAs($this->teacher);

        $folder = $this->folders()->createFolder($this->teacher->id, 'Silinəcək');
        $quizId = $this->makeQuiz($this->teacher->id, ['folder_id' => $folder->id]);

        $this->deleteJson("/api/v1/quiz-folders/{$folder->id}")->assertOk();

        // Qovluq silindi, quiz kökə düşdü
        $this->assertNull($this->folders()->find($folder->id));
        $this->assertNull($this->quizzes()->formData($quizId)['folder_id']);
    }

    public function test_folder_ownership_prevents_cross_teacher_access(): void
    {
        $folder = $this->folders()->createFolder($this->teacher->id, 'Şəxsi');
        $quizId = $this->makeQuiz($this->teacher->id, ['folder_id' => $folder->id]);

        Sanctum::actingAs($this->otherTeacher);

        $this->postJson("/api/v1/quiz-folders/{$folder->id}/rename", ['name' => 'X'])->assertStatus(403);
        $this->deleteJson("/api/v1/quiz-folders/{$folder->id}")->assertStatus(403);
        $this->postJson('/api/v1/quiz-folders/move-quiz', [
            'content_id' => $quizId,
            'folder_id' => null,
        ])->assertStatus(403);
    }

    public function test_quiz_form_renders_folder_select(): void
    {
        $folder = $this->folders()->createFolder($this->teacher->id, 'Qovluq');
        $quizId = $this->makeQuiz($this->teacher->id, ['folder_id' => $folder->id]);

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/quizzes/create')->assertOk()->getContent();
        $this->assertStringContainsString('name="folder_id"', $html);
        $this->assertStringContainsString('Qovluq', $html);

        $html = $this->get("/teacher/quizzes/{$quizId}/edit")->assertOk()->getContent();
        $this->assertStringContainsString('name="folder_id"', $html);
    }
}
