<?php

namespace Tests\Feature;

use App\Application\Lesson\LessonService;
use App\Application\Quiz\QuizService;
use App\Application\Workspace\WorkspaceService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Content\Content;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class WorkspaceFolderTest extends TestCase
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

    private function workspaces(): WorkspaceService
    {
        return app(WorkspaceService::class);
    }

    private function folders(): WorkspaceFolderService
    {
        return app(WorkspaceFolderService::class);
    }

    private function quizzes(): QuizService
    {
        return app(QuizService::class);
    }

    private function lessons(): LessonService
    {
        return app(LessonService::class);
    }

    private function makeWorkspace(int $teacherId, string $name = 'Sınaq Qrupu'): int
    {
        return $this->workspaces()->create($teacherId, $name)->id;
    }

    private function makeQuizInWorkspace(int $teacherId, int $workspaceId, ?int $wsFolderId = null): int
    {
        return $this->quizzes()->create($teacherId, [
            'title' => 'Workspace Quiz',
            'description' => 'İş sahəsi testi',
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $wsFolderId,
        ])->getKey();
    }

    private function makeLessonInWorkspace(int $teacherId, int $workspaceId, ?int $wsFolderId = null): int
    {
        return $this->lessons()->create($teacherId, [
            'title' => 'Workspace Dərsi',
            'description' => 'Video dərs',
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $wsFolderId,
        ])->getKey();
    }

    public function test_workspace_show_page_renders_folder_catalog_and_content(): void
    {
        $workspaceId = $this->makeWorkspace($this->teacher->id);
        $folder = $this->folders()->createFolder($workspaceId, 'Riyaziyyat', null, $this->teacher->id);
        $quizId = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $folder->id);
        $lessonId = $this->makeLessonInWorkspace($this->teacher->id, $workspaceId, $folder->id);
        $this->makeQuizInWorkspace($this->teacher->id, $workspaceId); // kökdə

        $this->actingAs($this->teacher);

        // Kök səhifə: qovluq + kökdəki məzmun + workspaceManager
        $html = $this->get("/teacher/workspaces/{$workspaceId}")->assertOk()->getContent();
        $this->assertStringContainsString('Riyaziyyat', $html);
        $this->assertStringContainsString('Workspace Quiz', $html);
        $this->assertStringContainsString('x-data="workspaceManager', $html);
        $this->assertStringContainsString('data-folder-action', $html);
        $this->assertStringContainsString('data-content-action', $html);

        // Alt qovluq səhifəsi: breadcrumb + folder-dəki məzmunlar
        $html = $this->get("/teacher/workspaces/{$workspaceId}?folder_id={$folder->id}")->assertOk()->getContent();
        $this->assertStringContainsString('Riyaziyyat', $html);
        $this->assertStringContainsString('Workspace Quiz', $html);
        $this->assertStringContainsString('Workspace Dərsi', $html);

        // Edit linkləri content növünə görə düzgündür
        $this->assertStringContainsString('/teacher/quizzes/'.$quizId.'/edit', $html);
        $this->assertStringContainsString('/teacher/lessons/'.$lessonId.'/edit', $html);
    }

    public function test_api_creates_workspace_folder_and_moves_content(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);

        // Qovluq yarat
        $created = $this->postJson("/api/v1/workspaces/{$workspaceId}/folders", ['name' => 'Cəbr'])
            ->assertStatus(201)->json('data');
        $folderId = $created['id'];

        // Quiz workspace + folder konteksti ilə yarat
        $quiz = $this->postJson('/api/v1/quizzes', [
            'title' => 'Cəbr Sınaqı',
            'workspace_id' => $workspaceId,
            'ws_folder_id' => $folderId,
        ])->assertStatus(201)->json('data');

        $form = $this->quizzes()->formData($quiz['content_id']);
        $this->assertEquals($workspaceId, $form['workspace_id']);
        $this->assertEquals($folderId, $form['ws_folder_id']);

        // Content-i workspace kökünə daşı
        $this->postJson('/api/v1/workspace-folders/move-content', [
            'content_id' => $quiz['content_id'],
            'workspace_id' => $workspaceId,
            'folder_id' => null,
        ])->assertOk();

        $form = $this->quizzes()->formData($quiz['content_id']);
        $this->assertEquals($workspaceId, $form['workspace_id']);
        $this->assertNull($form['ws_folder_id']);
    }

    public function test_api_renames_and_moves_folder(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);

        $parent = $this->folders()->createFolder($workspaceId, 'Kök Qovluq', null, $this->teacher->id);
        $child = $this->folders()->createFolder($workspaceId, 'Köhnə Ad', null, $this->teacher->id);

        // Adını dəyiş
        $this->postJson("/api/v1/workspaces/{$workspaceId}/folders/{$child->id}/rename", ['name' => 'Yeni Ad'])
            ->assertOk();
        $this->assertEquals('Yeni Ad', $this->folders()->find($child->id)->name);

        // Parent-a daşı
        $this->postJson("/api/v1/workspaces/{$workspaceId}/folders/{$child->id}/move", ['parent_id' => $parent->id])
            ->assertOk();
        $this->assertEquals($parent->id, $this->folders()->find($child->id)->parent_id);
    }

    public function test_api_deletes_folder_moving_content_to_root(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);

        $folder = $this->folders()->createFolder($workspaceId, 'Silinəcək', null, $this->teacher->id);
        $quizId = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $folder->id);

        $this->deleteJson("/api/v1/workspaces/{$workspaceId}/folders/{$folder->id}")->assertOk();

        // Qovluq silindi, content kökə düşdü (workspace-də qalır)
        $this->assertNull($this->folders()->find($folder->id));
        $form = $this->quizzes()->formData($quizId);
        $this->assertEquals($workspaceId, $form['workspace_id']);
        $this->assertNull($form['ws_folder_id']);
    }

    public function test_folder_ownership_prevents_cross_teacher_access(): void
    {
        $workspaceId = $this->makeWorkspace($this->teacher->id);
        $folder = $this->folders()->createFolder($workspaceId, 'Şəxsi', null, $this->teacher->id);
        $quizId = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $folder->id);

        Sanctum::actingAs($this->otherTeacher);

        $this->postJson("/api/v1/workspaces/{$workspaceId}/folders/{$folder->id}/rename", ['name' => 'X'])->assertStatus(403);
        $this->deleteJson("/api/v1/workspaces/{$workspaceId}/folders/{$folder->id}")->assertStatus(403);
        $this->postJson('/api/v1/workspace-folders/move-content', [
            'content_id' => $quizId,
            'workspace_id' => $workspaceId,
            'folder_id' => null,
        ])->assertStatus(403);
    }

    public function test_quiz_form_renders_workspace_context(): void
    {
        $workspaceId = $this->makeWorkspace($this->teacher->id);
        $folder = $this->folders()->createFolder($workspaceId, 'Dərs Qovluğu', null, $this->teacher->id);

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/quizzes/create?workspace_id={$workspaceId}&ws_folder_id={$folder->id}")
            ->assertOk()->getContent();
        $this->assertStringContainsString('name="workspace_id"', $html);
        $this->assertStringContainsString('name="ws_folder_id"', $html);
        $this->assertStringContainsString('Dərs Qovluğu', $html);
    }

    public function test_api_directory_returns_workspace_name_breadcrumbs_folders_and_contents(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id, 'Fizika Qrupu');
        $folder = $this->folders()->createFolder($workspaceId, 'Mexanika', null, $this->teacher->id);
        $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $folder->id);

        $json = $this->getJson("/api/v1/workspaces/{$workspaceId}/folders/directory?folder_id={$folder->id}")
            ->assertOk()->json('data');

        $this->assertEquals('Fizika Qrupu', $json['workspace_name']);
        $this->assertEquals('Mexanika', $json['breadcrumbs'][0]['name']);
        $this->assertCount(1, $json['contents']);
        $this->assertEquals('Workspace Quiz', $json['contents'][0]['title']);
        $this->assertEquals(Content::TYPE_QUIZ, $json['contents'][0]['type']);
        $this->assertTrue(isset($json['contents'][0]['type_label']));
    }
}
