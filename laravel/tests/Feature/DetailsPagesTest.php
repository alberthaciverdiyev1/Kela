<?php

namespace Tests\Feature;

use App\Application\Quiz\QuizService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class DetailsPagesTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;
    protected User $otherTeacher;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }

        $this->teacher = User::factory()->create();
        $this->teacher->assignRole(User::ROLE_TEACHER);

        $this->otherTeacher = User::factory()->create();
        $this->otherTeacher->assignRole(User::ROLE_TEACHER);
    }

    private function workspaces(): WorkspaceService
    {
        return app(WorkspaceService::class);
    }

    private function quizzes(): QuizService
    {
        return app(QuizService::class);
    }

    private function makeStudent(string $firstName = 'Ali'): User
    {
        $student = User::factory()->create(['first_name' => $firstName]);
        $student->assignRole(User::ROLE_STUDENT);

        return $student;
    }

    private function makeQuestion(string $text = '2+2 neçədir?'): \App\Domain\Question\Question
    {
        return \App\Domain\Question\Question::query()->create([
            'teacher_id' => $this->teacher->id,
            'text' => $text,
            'option_a' => '3',
            'option_b' => '4',
            'option_c' => '5',
            'option_d' => '6',
            'option_e' => null,
            'correct_option' => 1,
            'explanation' => 'İki ilə ikinin cəmi dörddür.',
        ]);
    }

    public function test_student_details_page_renders_profile_and_workspaces(): void
    {
        $student = $this->makeStudent('Əli');
        $wsId = $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu')->id;
        $this->workspaces()->attachStudents($this->teacher->id, $wsId, [$student->id]);

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/students/{$student->id}")->assertOk()->getContent();

        $this->assertStringContainsString('Əli', $html);
        $this->assertStringContainsString($student->email, $html);
        $this->assertStringContainsString('Üzv olduğu iş sahələri', $html);
        $this->assertStringContainsString('Sınaq Qrupu', $html);
        $this->assertStringContainsString('Geri', $html);
        $this->assertStringContainsString('Redaktə', $html);
    }

    public function test_student_details_page_404_for_unknown_student(): void
    {
        $this->actingAs($this->teacher);
        $this->get('/teacher/students/9999')->assertStatus(404);
    }

    public function test_student_details_page_shows_empty_workspaces_state(): void
    {
        $student = $this->makeStudent('Tənha');

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/students/{$student->id}")->assertOk()->getContent();
        $this->assertStringContainsString('İş sahəsi yoxdur', $html);
    }

    public function test_student_table_links_to_details(): void
    {
        $student = $this->makeStudent('Cavid');

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/students')->assertOk()->getContent();
        $this->assertStringContainsString("/teacher/students/{$student->id}", $html);
    }

    public function test_quiz_details_page_renders_info_and_questions(): void
    {
        $quiz = $this->quizzes()->create($this->teacher->id, [
            'title' => 'Riyaziyyat testi',
            'description' => 'İmtahan üçün',
            'is_published' => true,
        ]);

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/quizzes/{$quiz->content_id}")->assertOk()->getContent();

        $this->assertStringContainsString('Riyaziyyat testi', $html);
        $this->assertStringContainsString('İmtahan üçün', $html);
        $this->assertStringContainsString('Yayımlandı', $html);
        $this->assertStringContainsString('Suallar', $html);
        $this->assertStringContainsString('Geri', $html);
        $this->assertStringContainsString('Redaktə', $html);
    }

    public function test_quiz_details_shows_questions_with_options(): void
    {
        $question = \App\Domain\Question\Question::query()->create([
            'teacher_id' => $this->teacher->id,
            'text' => '2+2 neçədir?',
            'option_a' => '3',
            'option_b' => '4',
            'option_c' => '5',
            'option_d' => '6',
            'option_e' => null,
            'correct_option' => 1,
        ]);
        $quiz = $this->quizzes()->create($this->teacher->id, ['title' => 'Test']);
        $this->quizzes()->addQuestion($quiz->content_id, $question->id, $this->teacher->id);

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/quizzes/{$quiz->content_id}")->assertOk()->getContent();

        $this->assertStringContainsString('2+2 neçədir?', $html);
        $this->assertStringContainsString('B', $html);
        $this->assertStringContainsString('doğru', $html);
    }

    public function test_quiz_details_blocks_cross_teacher_access(): void
    {
        $quiz = $this->quizzes()->create($this->teacher->id, ['title' => 'Məxfi test']);

        $this->actingAs($this->otherTeacher);

        $this->get("/teacher/quizzes/{$quiz->content_id}")->assertStatus(403);
    }

    public function test_quiz_index_links_to_details(): void
    {
        $quiz = $this->quizzes()->create($this->teacher->id, ['title' => 'Linkli test']);

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/quizzes')->assertOk()->getContent();
        $this->assertStringContainsString("/teacher/quizzes/{$quiz->content_id}", $html);
    }

    public function test_question_details_page_renders_text_options_and_correct_answer(): void
    {
        $question = $this->makeQuestion('Azərbaycanın paytaxtı hansıdır?');

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/questions/{$question->id}")->assertOk()->getContent();

        $this->assertStringContainsString('Sual Detayı', $html);
        $this->assertStringContainsString('Azərbaycanın paytaxtı hansıdır?', $html);
        $this->assertStringContainsString('B', $html);
        $this->assertStringContainsString('Doğru cavab', $html);
        $this->assertStringContainsString('İki ilə ikinin cəmi dörddür.', $html);
        $this->assertStringContainsString('Geri', $html);
    }

    public function test_question_details_page_404_for_unknown_question(): void
    {
        $this->actingAs($this->teacher);
        $this->get('/teacher/questions/9999')->assertStatus(404);
    }

    public function test_question_details_page_blocks_cross_teacher_access(): void
    {
        $question = $this->makeQuestion('Məxfi sual');

        $this->actingAs($this->otherTeacher);

        $this->get("/teacher/questions/{$question->id}")->assertStatus(403);
    }

    public function test_question_table_links_to_details(): void
    {
        $question = $this->makeQuestion('Linkli sual');

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/questions')->assertOk()->getContent();
        $this->assertStringContainsString("/teacher/questions/{$question->id}", $html);
    }
}
