<?php

namespace Tests\Feature;

use App\Application\Question\QuestionService;
use App\Application\Quiz\QuizService;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class QuizEditorTest extends TestCase
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
    }

    private function makeQuiz(string $title = 'Sınaq'): int
    {
        return app(QuizService::class)->create($this->teacher->id, ['title' => $title])->getKey();
    }

    private function attach(int $quizId, int $questionId): void
    {
        app(QuizService::class)->addQuestion($quizId, $questionId, $this->teacher->id);
    }

    public function test_quiz_editor_renders_question_controls_and_fragment(): void
    {
        $this->actingAs($this->teacher);

        $quizId = $this->makeQuiz();
        $q = app(QuestionService::class)->create($this->teacher->id, ['text' => 'Sual', 'option_a' => 'a', 'option_b' => 'b']);
        $this->attach($quizId, $q->id);

        $html = $this->get("/teacher/quizzes/{$quizId}/edit")->assertOk()->getContent();

        $this->assertStringContainsString('data-content-id="'.$quizId.'"', $html);
        $this->assertStringContainsString('bank-btn', $html);
        $this->assertStringContainsString('questions-list', $html);
        $this->assertStringContainsString('Sual Bankına Get', $html);

        // Quiz redaktoru sual YARATMIR — inline sual formu olmamalıdır.
        $this->assertStringNotContainsString('add-question-btn', $html);
        $this->assertStringNotContainsString('Sual Əlavə Et', $html);
        $this->assertStringNotContainsString('showQuestion', $html);

        // Sual siyahısı fragment-də də mövcuddur
        $this->get("/teacher/quizzes/{$quizId}/questions")
            ->assertOk()
            ->assertSee('Sual')
            ->assertSee('data-question-action');
    }

    public function test_quiz_editor_adds_new_question_via_api(): void
    {
        Sanctum::actingAs($this->teacher);

        $quizId = $this->makeQuiz();

        $created = $this->postJson('/api/v1/questions', [
            'text' => '2+2=?',
            'option_a' => '3',
            'option_b' => '4',
            'correct_option' => 1,
        ])->assertStatus(201)->json('data');

        $this->postJson("/api/v1/quizzes/{$quizId}/questions", [
            'question_id' => $created['id'],
        ])->assertOk();

        $questions = app(QuizService::class)->questionList($quizId);
        $this->assertCount(1, $questions);
        $this->assertEquals('2+2=?', $questions[0]['text']);
        $this->assertEquals(1, $questions[0]['correct_option']);
    }

    public function test_quiz_editor_adds_from_bank_and_edits(): void
    {
        $quizId = $this->makeQuiz();
        $qsvc = app(QuestionService::class);
        $svc = app(QuizService::class);

        $bank = $qsvc->create($this->teacher->id, ['text' => 'Bank sualı', 'option_a' => 'a', 'option_b' => 'b', 'correct_option' => 0]);
        $q = $qsvc->create($this->teacher->id, ['text' => 'Mövcud', 'option_a' => 'a', 'option_b' => 'b', 'correct_option' => 0]);
        $this->attach($quizId, $q->id);

        Sanctum::actingAs($this->teacher);

        // Bankdan əlavə et
        $this->postJson("/api/v1/quizzes/{$quizId}/questions", ['question_id' => $bank->id])->assertOk();
        $this->assertCount(2, $svc->questionList($quizId));

        // Mövcud sualı API ilə düzləndir
        $this->putJson("/api/v1/questions/{$q->id}", [
            'text' => 'Yenilənmiş sual',
            'option_a' => 'x',
            'option_b' => 'y',
            'option_c' => 'z',
            'correct_option' => 2,
        ])->assertOk();

        $questions = $svc->questionList($quizId);
        $updated = collect($questions)->firstWhere('question_id', $q->id);
        $this->assertEquals('Yenilənmiş sual', $updated['text']);
        $this->assertEquals(2, $updated['correct_option']);
    }

    public function test_quiz_question_reorder_up_and_down_via_api(): void
    {
        Sanctum::actingAs($this->teacher);

        $quizId = $this->makeQuiz();
        $svc = app(QuizService::class);
        $qsvc = app(QuestionService::class);

        $q1 = $qsvc->create($this->teacher->id, ['text' => 'Bir', 'option_a' => 'a', 'option_b' => 'b']);
        $q2 = $qsvc->create($this->teacher->id, ['text' => 'İki', 'option_a' => 'a', 'option_b' => 'b']);
        $q3 = $qsvc->create($this->teacher->id, ['text' => 'Üç', 'option_a' => 'a', 'option_b' => 'b']);
        foreach ([$q1, $q2, $q3] as $q) {
            $this->attach($quizId, $q->id);
        }

        $this->assertEquals(['Bir', 'İki', 'Üç'], array_column($svc->questionList($quizId), 'text'));

        $this->postJson("/api/v1/quizzes/{$quizId}/questions/{$q2->id}/move", ['direction' => 'up'])->assertOk();
        $this->assertEquals(['İki', 'Bir', 'Üç'], array_column($svc->questionList($quizId), 'text'));

        $this->postJson("/api/v1/quizzes/{$quizId}/questions/{$q3->id}/move", ['direction' => 'down'])->assertOk();
        $this->assertEquals(['İki', 'Bir', 'Üç'], array_column($svc->questionList($quizId), 'text'));

        $this->postJson("/api/v1/quizzes/{$quizId}/questions/{$q2->id}/move", ['direction' => 'up'])->assertOk();
        $this->assertEquals(['İki', 'Bir', 'Üç'], array_column($svc->questionList($quizId), 'text'));
    }

    public function test_quiz_question_remove_via_api(): void
    {
        Sanctum::actingAs($this->teacher);

        $quizId = $this->makeQuiz();
        $svc = app(QuizService::class);
        $q = app(QuestionService::class)->create($this->teacher->id, ['text' => 'Silinəcək', 'option_a' => 'a', 'option_b' => 'b']);
        $this->attach($quizId, $q->id);

        $this->assertCount(1, $svc->questionList($quizId));

        $this->deleteJson("/api/v1/quizzes/{$quizId}/questions/{$q->id}")->assertOk();

        $this->assertCount(0, $svc->questionList($quizId));
    }

    public function test_quiz_available_questions_excludes_attached(): void
    {
        $quizId = $this->makeQuiz();
        $svc = app(QuizService::class);
        $qsvc = app(QuestionService::class);

        $q1 = $qsvc->create($this->teacher->id, ['text' => 'Daxil olan', 'option_a' => 'a', 'option_b' => 'b']);
        $q2 = $qsvc->create($this->teacher->id, ['text' => 'Bankda qalan', 'option_a' => 'a', 'option_b' => 'b']);
        $this->attach($quizId, $q1->id);

        $this->assertEquals([$q2->id], $svc->availableQuestionIds($quizId, $this->teacher->id));

        $this->attach($quizId, $q2->id);
        $this->assertEquals([], $svc->availableQuestionIds($quizId, $this->teacher->id));
    }
}
