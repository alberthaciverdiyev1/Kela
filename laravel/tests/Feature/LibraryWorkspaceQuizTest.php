<?php

namespace Tests\Feature;

use App\Application\Library\LibraryService;
use App\Application\Question\QuestionService;
use App\Application\Quiz\QuizService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\User\User;
use App\Filament\Pages\Library\LibraryPage;
use App\Filament\Resources\Quizzes\Pages\EditQuiz;
use App\Filament\Resources\Workspaces\Pages\ViewWorkspace;
use Filament\Facades\Filament;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Livewire\Livewire;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class LibraryWorkspaceQuizTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;
    protected User $admin;

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

        Filament::setCurrentPanel('admin');
    }

    public function test_library_page_renders_and_crud(): void
    {
        $this->actingAs($this->teacher);

        $this->get('/admin/library')->assertOk();

        $service = app(LibraryService::class);

        // Create folder + lesson + quiz
        $folder = $service->createFolder($this->teacher->id, 'Dersler');
        $lessonId = $service->createContent($this->teacher->id, ['title' => 'Riyaziyyat', 'type' => 0], $folder->id);
        $quizId = $service->createContent($this->teacher->id, ['title' => 'Sınaq', 'type' => 1], $folder->id);

        // Livewire interaction
        Livewire::test(LibraryPage::class)
            ->assertOk()
            ->call('openFolder', $folder->id)
            ->assertCount('contents', 2)
            ->call('openRoot')
            ->assertCount('folders', 1);

        // Counts
        $counts = $service->counts($this->teacher->id);
        $this->assertEquals(1, $counts[0]);
        $this->assertEquals(1, $counts[1]);
    }

    public function test_quiz_editor_renders_and_adds_question(): void
    {
        $this->actingAs($this->teacher);

        $service = app(LibraryService::class);
        $quizId = $service->createContent($this->teacher->id, ['title' => 'Sınaq', 'type' => 1]);

        $this->get("/admin/quizzes/{$quizId}/edit")->assertOk();

        // Add question via service
        $question = app(QuestionService::class)->create($this->teacher->id, [
            'text' => '2+2=?',
            'option_a' => '3',
            'option_b' => '4',
            'correct_option' => 1,
        ]);
        app(QuizService::class)->addQuestion($quizId, $question->id, $this->teacher->id);

        Livewire::test(EditQuiz::class, ['record' => $quizId])
            ->assertOk()
            ->assertSet('record.content_id', $quizId);

        $questions = app(QuizService::class)->questionList($quizId);
        $this->assertCount(1, $questions);
        $this->assertEquals('2+2=?', $questions[0]['text']);
    }

    public function test_workspace_view_renders_file_manager_tree(): void
    {
        $this->actingAs($this->teacher);

        $lib = app(LibraryService::class);
        $ws = app(WorkspaceService::class);

        $lessonId = $lib->createContent($this->teacher->id, ['title' => 'Riyaziyyat', 'type' => 0]);
        $workspace = $ws->create($this->teacher->id, 'Sinif 3A');
        $ws->addContent($this->teacher->id, $workspace->id, $lessonId);

        $this->get("/admin/workspaces/{$workspace->id}")->assertOk();

        Livewire::test(ViewWorkspace::class, ['record' => $workspace->id])
            ->assertOk()
            ->assertCount('contents', 1);
    }

    public function test_library_page_role_restricted(): void
    {
        $student = User::factory()->create();
        $student->assignRole(User::ROLE_STUDENT);

        $this->actingAs($student);
        $this->get('/admin/library')->assertForbidden();
    }
}
