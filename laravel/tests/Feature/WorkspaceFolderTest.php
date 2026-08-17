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

    public function test_workspace_index_page_offers_list_grid_toggle(): void
    {
        $this->makeWorkspace($this->teacher->id, 'Sınaq Qrupu');

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/workspaces')->assertOk()->getContent();
        $this->assertStringContainsString('Sınaq Qrupu', $html);
        $this->assertStringContainsString('x-data="workspaceList(', $html);

        // List/Grid görünüm keçidi və grid kartları mövcuddur
        $this->assertStringContainsString('setViewMode(\'list\')', $html);
        $this->assertStringContainsString('setViewMode(\'grid\')', $html);
        $this->assertStringContainsString('viewMode === \'list\'', $html);
        $this->assertStringContainsString('viewMode === \'grid\'', $html);
        $this->assertStringContainsString('Liste görünümü', $html);
        $this->assertStringContainsString('Grid görünümü', $html);
        $this->assertStringContainsString('grid-cols-6', $html);
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
        $this->assertStringContainsString('data-kind="folder"', $html);
        $this->assertStringContainsString('data-kind="content"', $html);

        // Sağ-tık kontekst menyusu sətirlərdə qurulur
        $this->assertStringContainsString('@contextmenu.prevent', $html);
        $this->assertStringContainsString('openRowContextMenu', $html);
        $this->assertStringContainsString('ctxMenu.show', $html);

        // List/Grid görünüm keçidi mövcuddur
        $this->assertStringContainsString('setViewMode(\'list\')', $html);
        $this->assertStringContainsString('setViewMode(\'grid\')', $html);
        $this->assertStringContainsString('viewMode === \'list\'', $html);
        $this->assertStringContainsString('viewMode === \'grid\'', $html);
        $this->assertStringContainsString('Liste görünümü', $html);
        $this->assertStringContainsString('Grid görünümü', $html);

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

    public function test_api_deletes_folder_with_its_contents(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);

        // Qovluq ağacı: Silinəcək → Alt; bir də kontrol (kökdə) qovluq
        $folder = $this->folders()->createFolder($workspaceId, 'Silinəcək', null, $this->teacher->id);
        $child = $this->folders()->createFolder($workspaceId, 'Alt', $folder->id, $this->teacher->id);
        $other = $this->folders()->createFolder($workspaceId, 'Kontrol', null, $this->teacher->id);

        // İçərilər: Silinəcək-də quiz, Alt-da dərs, Kontrol-da quiz
        $quizId = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $folder->id);
        $lessonId = $this->makeLessonInWorkspace($this->teacher->id, $workspaceId, $child->id);
        $otherQuizId = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $other->id);

        $this->deleteJson("/api/v1/workspaces/{$workspaceId}/folders/{$folder->id}")->assertOk();

        // Qovluq ağacı silindi
        $this->assertNull($this->folders()->find($folder->id));
        $this->assertNull($this->folders()->find($child->id));

        // İçindəki məzmunlar da silindi (quiz + lesson + content)
        $this->assertNull($this->quizzes()->find($quizId));
        $this->assertNull(app(\App\Domain\Lesson\LessonRepository::class)->find($lessonId));
        $this->assertNull(app(\App\Domain\Content\ContentRepository::class)->find($quizId));
        $this->assertNull(app(\App\Domain\Content\ContentRepository::class)->find($lessonId));

        // Kontrol qovluq və içindəki məzmun toxunulmadı
        $this->assertNotNull($this->folders()->find($other->id));
        $this->assertNotNull($this->quizzes()->find($otherQuizId));
        $form = $this->quizzes()->formData($otherQuizId);
        $this->assertEquals($workspaceId, $form['workspace_id']);
        $this->assertEquals($other->id, $form['ws_folder_id']);
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
        $this->postJson('/api/v1/workspace-folders/remove-content', [
            'content_id' => $quizId,
        ])->assertStatus(403);
        $this->postJson("/api/v1/workspaces/{$workspaceId}/folders/{$folder->id}/remove")->assertStatus(403);
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

    public function test_api_adds_existing_content_to_workspace(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);
        $folder = $this->folders()->createFolder($workspaceId, 'Yeni Qovluq', null, $this->teacher->id);

        // Workspace xaricində (kitabxanada) yaradılmış quiz və dərs
        $quizId = $this->quizzes()->create($this->teacher->id, ['title' => 'Köhnə Quiz'])->getKey();
        $lessonId = $this->lessons()->create($this->teacher->id, ['title' => 'Köhnə Dərs'])->getKey();

        // available-contents endpoint hər ikisini göstərir
        $available = $this->getJson("/api/v1/workspaces/{$workspaceId}/available-contents")
            ->assertOk()->json('data');
        $this->assertCount(2, $available);
        $this->assertEqualsCanonicalizing(
            ['Köhnə Quiz', 'Köhnə Dərs'],
            array_column($available, 'title'),
        );

        // İkisini də workspace qovluğuna əlavə et
        foreach ([$quizId, $lessonId] as $contentId) {
            $this->postJson('/api/v1/workspace-folders/move-content', [
                'content_id' => $contentId,
                'workspace_id' => $workspaceId,
                'folder_id' => $folder->id,
            ])->assertOk();
        }

        // İndi qovluğun kataloqunda görünürlər
        $dir = $this->folders()->directory($workspaceId, $folder->id, $this->teacher->id);
        $this->assertCount(2, $dir['contents']);
        $titles = array_column($dir['contents'], 'title');
        $this->assertContains('Köhnə Quiz', $titles);
        $this->assertContains('Köhnə Dərs', $titles);

        // Artıq available siyahısında yoxdur
        $available = $this->getJson("/api/v1/workspaces/{$workspaceId}/available-contents")
            ->assertOk()->json('data');
        $this->assertCount(0, $available);
    }

    public function test_api_available_contents_grouped_by_bank_folder(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);

        // Bank qovluqları: Riyaziyyat → Cəbr, bir də kökdə quiz
        $quizFolderSvc = app(\App\Application\QuizFolder\QuizFolderService::class);
        $math = $quizFolderSvc->createFolder($this->teacher->id, 'Riyaziyyat');
        $algebra = $quizFolderSvc->createFolder($this->teacher->id, 'Cəbr', $math->id);

        $this->quizzes()->create($this->teacher->id, [
            'title' => 'Cəbr Quiz', 'folder_id' => $algebra->id,
        ])->getKey();
        $this->quizzes()->create($this->teacher->id, [
            'title' => 'Kök Quiz', 'folder_id' => null,
        ])->getKey();

        $available = $this->getJson("/api/v1/workspaces/{$workspaceId}/available-contents")
            ->assertOk()->json('data');

        $byTitle = collect($available)->keyBy('title');

        // Kökdəki quiz üçün boş path
        $this->assertSame([], $byTitle['Kök Quiz']['folder_path']);

        // Cəbr altındakı quiz üçün path kökdən yarpağa
        $this->assertSame(['Riyaziyyat', 'Cəbr'], $byTitle['Cəbr Quiz']['folder_path']);
        $this->assertSame([(int) $math->id, (int) $algebra->id], $byTitle['Cəbr Quiz']['folder_path_ids']);

        // Kökdəki quiz üçün ids də boşdur
        $this->assertSame([], $byTitle['Kök Quiz']['folder_path_ids']);
    }

    public function test_api_adds_bank_folder_with_structure_to_workspace(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);
        $target = $this->folders()->createFolder($workspaceId, 'Fənn Testləri', null, $this->teacher->id);

        // Bank qovluq ağacı: Riyaziyyat → Cəbr; bir də kənar (kontrol) qovluq
        $quizFolderSvc = app(\App\Application\QuizFolder\QuizFolderService::class);
        $math = $quizFolderSvc->createFolder($this->teacher->id, 'Riyaziyyat');
        $algebra = $quizFolderSvc->createFolder($this->teacher->id, 'Cəbr', $math->id);
        $other = $quizFolderSvc->createFolder($this->teacher->id, 'Fizika');

        $quiz1 = $this->quizzes()->create($this->teacher->id, ['title' => 'Sinaq 1', 'folder_id' => $math->id])->getKey();
        $quiz2 = $this->quizzes()->create($this->teacher->id, ['title' => 'Sinaq 2', 'folder_id' => $algebra->id])->getKey();
        $quiz3 = $this->quizzes()->create($this->teacher->id, ['title' => 'Fizika Quiz', 'folder_id' => $other->id])->getKey();

        $json = $this->postJson('/api/v1/workspace-folders/add-folder', [
            'folder_type' => 'quiz',
            'bank_folder_id' => $math->id,
            'workspace_id' => $workspaceId,
            'folder_id' => $target->id,
        ])->assertOk()->json();

        $this->assertSame(2, $json['folders']);  // Riyaziyyat + Cəbr
        $this->assertSame(2, $json['contents']); // hər ikisi kütüphanədə idi

        // Workspace qovluq strukturu hədəf altında əks olundu
        $wsFolders = app(\App\Domain\WorkspaceFolder\WorkspaceFolderRepository::class)->allFoldersFor($workspaceId);
        $mathWs = $wsFolders->firstWhere('name', 'Riyaziyyat');
        $algebraWs = $wsFolders->firstWhere('name', 'Cəbr');

        $this->assertNotNull($mathWs);
        $this->assertNotNull($algebraWs);
        $this->assertEquals($target->id, $mathWs->parent_id);
        $this->assertEquals($mathWs->id, $algebraWs->parent_id);
        $this->assertNull($wsFolders->firstWhere('name', 'Fizika')); // kontrol toxunulmadı

        // Məzmunlar müvafiq workspace qovluğuna düşdü
        $f1 = $this->quizzes()->formData($quiz1);
        $f2 = $this->quizzes()->formData($quiz2);
        $f3 = $this->quizzes()->formData($quiz3);

        $this->assertEquals($workspaceId, $f1['workspace_id']);
        $this->assertEquals($mathWs->id, $f1['ws_folder_id']);
        $this->assertEquals($workspaceId, $f2['workspace_id']);
        $this->assertEquals($algebraWs->id, $f2['ws_folder_id']);
        $this->assertNull($f3['workspace_id']); // kənar qovluqdakı quiz hələ kütüphanədə
        $this->assertNull($f3['ws_folder_id']);
    }

    public function test_api_add_folder_reuses_same_name_folder_and_skips_attached(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);

        $quizFolderSvc = app(\App\Application\QuizFolder\QuizFolderService::class);
        $bank = $quizFolderSvc->createFolder($this->teacher->id, 'Riyaziyyat');

        // Bank qovluğunda 2 quiz: biri artıq başqa workspace-də, biri kütüphanədə
        $unattached = $this->quizzes()->create($this->teacher->id, ['title' => 'Yeni Quiz', 'folder_id' => $bank->id])->getKey();
        $otherWsId = $this->makeWorkspace($this->teacher->id, 'Digər Qrup');
        $attached = $this->quizzes()->create($this->teacher->id, [
            'title' => 'Bağlı Quiz', 'folder_id' => $bank->id,
            'workspace_id' => $otherWsId,
        ])->getKey();

        // İlk əlavə: "Riyaziyyat" workspace qovluğu yaradılır
        $this->postJson('/api/v1/workspace-folders/add-folder', [
            'folder_type' => 'quiz',
            'bank_folder_id' => $bank->id,
            'workspace_id' => $workspaceId,
            'folder_id' => null,
        ])->assertOk();

        // İkinci əlavə eyni adlı qovluğu təkrarlamır
        $json = $this->postJson('/api/v1/workspace-folders/add-folder', [
            'folder_type' => 'quiz',
            'bank_folder_id' => $bank->id,
            'workspace_id' => $workspaceId,
            'folder_id' => null,
        ])->assertOk()->json();

        $this->assertSame(1, $json['folders']); // yenidən yaratmadı — eyni qovluğu qaytardı
        $this->assertSame(0, $json['contents']);

        $wsFolders = app(\App\Domain\WorkspaceFolder\WorkspaceFolderRepository::class)->allFoldersFor($workspaceId);
        $this->assertCount(1, $wsFolders->where('name', 'Riyaziyyat'));

        // Kütüphanədəki quiz workspace-ə düşdü, bağlı olan toxunulmadı
        $f = $this->quizzes()->formData($unattached);
        $this->assertEquals($workspaceId, $f['workspace_id']);
        $this->assertNotNull($f['ws_folder_id']);

        $attachedForm = $this->quizzes()->formData($attached);
        $this->assertEquals($otherWsId, $attachedForm['workspace_id']);
    }

    public function test_api_removes_content_from_workspace_to_library(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);
        $folder = $this->folders()->createFolder($workspaceId, 'Qovluq', null, $this->teacher->id);

        // Workspace qovluğunda quiz + kökdə dərs
        $quizId = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $folder->id);
        $lessonId = $this->makeLessonInWorkspace($this->teacher->id, $workspaceId);

        // Quiz workspace-dən çıxarılır → kütüphanəyə dönür, qovluqdan da çıxar
        $this->postJson('/api/v1/workspace-folders/remove-content', ['content_id' => $quizId])->assertOk();

        $form = $this->quizzes()->formData($quizId);
        $this->assertNull($form['workspace_id']);
        $this->assertNull($form['ws_folder_id']);

        // Artıq available siyahısında yenidən görünür
        $available = $this->getJson("/api/v1/workspaces/{$workspaceId}/available-contents")
            ->assertOk()->json('data');
        $titles = array_column($available, 'title');
        $this->assertContains('Workspace Quiz', $titles);
        $this->assertNotContains('Workspace Dərsi', $titles); // dərs hələ workspace-dədir

        // Dərs də workspace-dən çıxarıla bilər
        $this->postJson('/api/v1/workspace-folders/remove-content', ['content_id' => $lessonId])->assertOk();

        // İkinci dəfə çıxarmaq xətadır — artıq workspace-də deyil
        $this->postJson('/api/v1/workspace-folders/remove-content', ['content_id' => $lessonId])->assertStatus(422);
    }

    public function test_api_removes_folder_tree_with_contents_to_library(): void
    {
        Sanctum::actingAs($this->teacher);
        $workspaceId = $this->makeWorkspace($this->teacher->id);

        // Qovluq ağacı: Riyaziyyat → Cəbr; bir də başqa (kontrol) qovluq
        $math = $this->folders()->createFolder($workspaceId, 'Riyaziyyat', null, $this->teacher->id);
        $algebra = $this->folders()->createFolder($workspaceId, 'Cəbr', $math->id, $this->teacher->id);
        $other = $this->folders()->createFolder($workspaceId, 'Fizika', null, $this->teacher->id);

        $quizInMath = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $math->id);
        $lessonInAlgebra = $this->makeLessonInWorkspace($this->teacher->id, $workspaceId, $algebra->id);
        $quizInOther = $this->makeQuizInWorkspace($this->teacher->id, $workspaceId, $other->id);

        // Riyaziyyat + alt ağacı workspace-dən çıxar
        $this->postJson("/api/v1/workspaces/{$workspaceId}/folders/{$math->id}/remove")->assertOk();

        // Qovluq ağacı silindi
        $this->assertNull($this->folders()->find($math->id));
        $this->assertNull($this->folders()->find($algebra->id));

        // Alt ağacdakı məzmunlar kütüphanəyə döndü
        $f1 = $this->quizzes()->formData($quizInMath);
        $f3 = $this->quizzes()->formData($quizInOther);
        $lesson = Content::find($lessonInAlgebra);

        $this->assertNull($f1['workspace_id']);
        $this->assertNull($f1['ws_folder_id']);
        $this->assertNull($lesson->workspace_id);
        $this->assertNull($lesson->folder_id);

        // Kontrol qovluq və içindəki məzmun toxunulmadı
        $this->assertNotNull($this->folders()->find($other->id));
        $this->assertEquals($workspaceId, $f3['workspace_id']);
        $this->assertEquals($other->id, $f3['ws_folder_id']);
    }

    public function test_workspace_show_page_offers_add_existing_content(): void
    {
        $workspaceId = $this->makeWorkspace($this->teacher->id);
        $this->quizzes()->create($this->teacher->id, ['title' => 'Kütüphanədə Quiz'])->getKey();
        $this->lessons()->create($this->teacher->id, ['title' => 'Kütüphanədə Dərs'])->getKey();

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/workspaces/{$workspaceId}")->assertOk()->getContent();
        $this->assertStringContainsString('Məzmun əlavə et', $html);
        $this->assertStringContainsString('openContentAdd', $html);
        $this->assertStringContainsString('Kütüphanədə Quiz', $html);
        $this->assertStringContainsString('Kütüphanədə Dərs', $html);

        // Toolbar-da artıq Yeni Quiz / Yeni Dərs butonu yoxdur
        $this->assertStringNotContainsString('Yeni Quiz', $html);
        $this->assertStringNotContainsString('Yeni Dərs', $html);

        // Modal axtarış + tip filtri var
        $this->assertStringContainsString('x-model="contentSearch"', $html);
        $this->assertStringContainsString('x-model="contentTypeFilter"', $html);
        $this->assertStringContainsString('option value="1"', $html);
        $this->assertStringContainsString('option value="0"', $html);

        // Modal ağac görünümü: qovluq başlıqları + məzmun sətirləri
        $this->assertStringContainsString('x-for="(row, i) in visibleContentRows"', $html);
        $this->assertStringContainsString('row.kind === \'folder\'', $html);
        $this->assertStringContainsString('row.kind === \'content\'', $html);

        // Sadələşdirilmiş dizayn: collapse interaksiyası tamamilə yoxdur
        $this->assertStringNotContainsString('toggleContentFolder', $html);
        $this->assertStringNotContainsString('row.collapsed', $html);
        $this->assertStringNotContainsString('chevron-right', $html);

        // Bütöv qovluq əlavəsi: qovluq checkbox + seçim metodu
        $this->assertStringContainsString('isFolderSelected(row)', $html);
        $this->assertStringContainsString('toggleFolderSelection(row)', $html);
        $this->assertStringContainsString('isContentCoveredByFolder', $html);
        $this->assertStringContainsString('selectionSummary', $html);

        // Kaskad seçim: üst qovluq alt ağacı yönetir (seçilmiş ata → alt qovluq covered)
        $this->assertStringContainsString('isFolderCovered(row)', $html);
        $this->assertStringContainsString('isFolderSelected(row) || isFolderCovered(row)', $html);

        // Modal yeni dizayn elementləri
        $this->assertStringContainsString('Haraya:', $html);
        $this->assertStringContainsString('updateContentSelection($event)', $html);
        $this->assertStringContainsString('selectedContentCount', $html);
    }
}
