<?php

namespace Tests\Feature;

use App\Application\Homework\HomeworkService;
use App\Application\Question\QuestionService;
use App\Application\Quiz\QuizService;
use App\Domain\Homework\Homework;
use App\Domain\Homework\HomeworkQuestion;
use App\Domain\Homework\Values\HomeworkQuestionType;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class HomeworkTest extends TestCase
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

    private function homeworks(): HomeworkService
    {
        return app(HomeworkService::class);
    }

    private function quizzes(): QuizService
    {
        return app(QuizService::class);
    }

    private function questions(): QuestionService
    {
        return app(QuestionService::class);
    }

    private function makeQuizWithQuestions(int $teacherId): array
    {
        $quizId = $this->quizzes()->create($teacherId, [
            'title' => 'Sınaq Quizi',
            'description' => 'Riyaziyyat',
        ])->getKey();

        $q1 = $this->questions()->create($teacherId, [
            'text' => '2+2 neçədir?',
            'option_a' => '3',
            'option_b' => '4',
            'option_c' => '5',
            'correct_option' => 1,
        ]);
        $q2 = $this->questions()->create($teacherId, [
            'text' => '3+3 neçədir?',
            'option_a' => '5',
            'option_b' => '6',
            'correct_option' => 1,
        ]);

        $this->quizzes()->addQuestion($quizId, $q1->id, (int) auth()->id());
        $this->quizzes()->addQuestion($quizId, $q2->id, (int) auth()->id());

        return ['quiz_id' => $quizId, 'q1_id' => $q1->id, 'q2_id' => $q2->id];
    }

    public function test_homework_index_page_renders(): void
    {
        $this->homeworks()->create($this->teacher->id, [
            'title' => 'Cəbr Ev Tapşırığı',
            'description' => 'Həftəsonu üçün',
            'questions' => [
                ['type' => HomeworkQuestionType::TASK, 'text' => 'x² = 25, x-i tap'],
            ],
        ]);

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/homeworks')->assertOk()->getContent();
        $this->assertStringContainsString('Cəbr Ev Tapşırığı', $html);
        $this->assertStringContainsString('Yeni Ev Tapşırığı', $html);
        $this->assertStringContainsString('1 sual', $html);
    }

    public function test_teacher_creates_homework_with_hand_written_task_question(): void
    {
        $this->actingAs($this->teacher);

        $this->post('/teacher/homeworks', [
            'title' => 'Tarix tapşırığı',
            'description' => 'Osmanlı tarixi',
            'is_published' => '1',
            'questions_json' => json_encode([
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Osmanlının quruluş ilini yazın'],
            ]),
        ])->assertRedirect();

        $homework = Homework::first();
        $this->assertNotNull($homework);
        $this->assertEquals('Tarix tapşırığı', $homework->title);
        $this->assertTrue($homework->is_published);

        $question = $homework->questions()->first();
        $this->assertNotNull($question);
        $this->assertEquals(HomeworkQuestionType::TASK, (int) $question->type);
        $this->assertEquals(1, (int) $question->position);
        $this->assertEquals('Osmanlının quruluş ilini yazın', $question->text);

        // Əl ilə yazılan sual variantsız olmalıdır
        $this->assertNull($question->option_a);
        $this->assertNull($question->option_b);
        $this->assertNull($question->correct_option);
        $this->assertNull($question->source_question_id);
    }

    public function test_teacher_creates_homework_with_questions_from_quiz(): void
    {
        $this->actingAs($this->teacher);
        $data = $this->makeQuizWithQuestions($this->teacher->id);

        $this->post('/teacher/homeworks', [
            'title' => 'Riyaziyyat ev tapşırığı',
            'questions_json' => json_encode([
                [
                    'type' => HomeworkQuestionType::QUIZ,
                    'text' => '2+2 neçədir?',
                    'option_a' => '3',
                    'option_b' => '4',
                    'option_c' => '5',
                    'correct_option' => 1,
                    'source_question_id' => $data['q1_id'],
                    'source_quiz_id' => $data['quiz_id'],
                ],
                ['type' => HomeworkQuestionType::TASK, 'text' => 'İki rəqəmli bölmə yazın'],
            ]),
        ])->assertRedirect();

        $homework = Homework::first();
        $this->assertNotNull($homework);

        $quizQuestion = $homework->questions()->where('type', HomeworkQuestionType::QUIZ)->first();
        $this->assertNotNull($quizQuestion);
        $this->assertEquals('2+2 neçədir?', $quizQuestion->text);
        $this->assertEquals('4', $quizQuestion->option_b);
        $this->assertEquals(1, (int) $quizQuestion->correct_option);
        $this->assertEquals($data['q1_id'], (int) $quizQuestion->source_question_id);
        $this->assertEquals($data['quiz_id'], (int) $quizQuestion->source_quiz_id);

        $taskQuestion = $homework->questions()->where('type', HomeworkQuestionType::TASK)->first();
        $this->assertNotNull($taskQuestion);
        $this->assertNull($taskQuestion->option_a);

        // Sıralama: quiz sualı 1, tapşırıq 2
        $this->assertEquals(1, (int) $homework->questions()->orderBy('position')->first()->position);
        $this->assertCount(2, $homework->questions);
    }

    public function test_homework_show_page_renders_both_question_types(): void
    {
        $this->actingAs($this->teacher);
        $data = $this->makeQuizWithQuestions($this->teacher->id);

        $homework = $this->homeworks()->create($this->teacher->id, [
            'title' => 'Qarışıq tapşırıq',
            'questions' => [
                ['type' => HomeworkQuestionType::QUIZ, 'text' => '2+2 neçədir?', 'option_a' => '3', 'option_b' => '4', 'correct_option' => 1, 'source_question_id' => $data['q1_id'], 'source_quiz_id' => $data['quiz_id']],
                ['type' => HomeworkQuestionType::TASK, 'text' => 'İnsanlıq tarixini təsvir edin'],
            ],
        ]);

        $html = $this->get("/teacher/homeworks/{$homework->id}")->assertOk()->getContent();
        $this->assertStringContainsString('Qarışıq tapşırıq', $html);
        $this->assertStringContainsString('2+2 neçədir?', $html);
        $this->assertStringContainsString('İnsanlıq tarixini təsvir edin', $html);
        $this->assertStringContainsString('Quiz sualı', $html);
        $this->assertStringContainsString('Tapşırıq', $html);
        $this->assertStringContainsString('Redaktə', $html);
    }

    public function test_homework_edit_page_prefills_existing_questions(): void
    {
        $this->actingAs($this->teacher);

        $homework = $this->homeworks()->create($this->teacher->id, [
            'title' => 'Redaktə olunacaq',
            'questions' => [
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Köhnə tapşırıq'],
            ],
        ]);

        $html = $this->get("/teacher/homeworks/{$homework->id}/edit")->assertOk()->getContent();
        $this->assertStringContainsString('Redaktə olunacaq', $html);
        $this->assertStringContainsString('Köhnə tapşırıq', $html);
        $this->assertStringContainsString('x-data="homeworkEditor', $html);
        $this->assertStringContainsString('Quizdən əlavə et', $html);
        $this->assertStringContainsString('Əl ilə tapşırıq yaz', $html);
        $this->assertStringContainsString('name="questions_json"', $html);
    }

    public function test_homework_update_replaces_questions(): void
    {
        $this->actingAs($this->teacher);

        $homework = $this->homeworks()->create($this->teacher->id, [
            'title' => 'Köhnə başlıq',
            'questions' => [
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Köhnə sual'],
            ],
        ]);

        $this->post("/teacher/homeworks/{$homework->id}", [
            'title' => 'Yeni başlıq',
            'is_published' => '1',
            'questions_json' => json_encode([
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Yeni sual 1'],
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Yeni sual 2'],
            ]),
        ])->assertRedirect();

        $homework->refresh();
        $this->assertEquals('Yeni başlıq', $homework->title);
        $this->assertTrue($homework->is_published);
        $this->assertCount(2, $homework->questions);
        $this->assertEquals(['Yeni sual 1', 'Yeni sual 2'], $homework->questions()->orderBy('position')->pluck('text')->all());
    }

    public function test_homework_delete_removes_homework_and_questions(): void
    {
        $this->actingAs($this->teacher);

        $homework = $this->homeworks()->create($this->teacher->id, [
            'title' => 'Silinəcək',
            'questions' => [
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Sual'],
            ],
        ]);
        $questionId = $homework->questions()->first()->id;

        $this->delete("/teacher/homeworks/{$homework->id}")->assertRedirect();

        $this->assertNull(Homework::find($homework->id));
        $this->assertNull(HomeworkQuestion::find($questionId));
    }

    public function test_quiz_questions_endpoint_returns_quiz_questions(): void
    {
        $this->actingAs($this->teacher);
        $data = $this->makeQuizWithQuestions($this->teacher->id);

        $json = $this->getJson("/teacher/homeworks/quiz-questions/{$data['quiz_id']}")->assertOk()->json();
        $this->assertCount(2, $json['questions']);
        $this->assertEquals('2+2 neçədir?', $json['questions'][0]['text']);
        $this->assertEquals(['A' => '3', 'B' => '4', 'C' => '5'], $json['questions'][0]['options']);
        $this->assertEquals(1, $json['questions'][0]['correct_option']);
    }

    public function test_quiz_questions_endpoint_blocks_other_teacher(): void
    {
        $this->actingAs($this->teacher);
        $data = $this->makeQuizWithQuestions($this->teacher->id);

        $this->actingAs($this->otherTeacher);
        $this->getJson("/teacher/homeworks/quiz-questions/{$data['quiz_id']}")->assertStatus(403);
    }

    public function test_homework_ownership_prevents_cross_teacher_access(): void
    {
        $this->actingAs($this->teacher);
        $homework = $this->homeworks()->create($this->teacher->id, [
            'title' => 'Şəxsi tapşırıq',
            'questions' => [
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Gizli sual'],
            ],
        ]);

        $this->actingAs($this->otherTeacher);

        $this->get("/teacher/homeworks/{$homework->id}")->assertStatus(403);
        $this->get("/teacher/homeworks/{$homework->id}/edit")->assertStatus(403);
        $this->post("/teacher/homeworks/{$homework->id}", ['title' => 'X', 'questions_json' => '[]'])->assertStatus(403);
        $this->delete("/teacher/homeworks/{$homework->id}")->assertStatus(403);
    }

    public function test_admin_can_access_any_homework(): void
    {
        $this->actingAs($this->teacher);
        $homework = $this->homeworks()->create($this->teacher->id, [
            'title' => 'Admin görə bilər',
            'questions' => [
                ['type' => HomeworkQuestionType::TASK, 'text' => 'Sual'],
            ],
        ]);

        $this->actingAs($this->admin);
        $this->get("/teacher/homeworks/{$homework->id}")->assertOk();
    }

    public function test_quiz_picker_returns_nested_quizzes_with_folder_paths(): void
    {
        $this->actingAs($this->teacher);

        $folders = app(\App\Application\QuizFolder\QuizFolderService::class);
        $fenn = $folders->createFolder($this->teacher->id, 'Fənn Testləri');
        $riyaziyyat = $folders->createFolder($this->teacher->id, 'Riyaziyyat', $fenn->id);

        $quizId = $this->quizzes()->create($this->teacher->id, [
            'title' => 'İç-içə Quiz',
            'description' => null,
        ])->getKey();
        $folders->moveQuiz($this->teacher->id, $quizId, $riyaziyyat->id);

        $json = $this->getJson('/api/v1/quiz-folders/picker')->assertOk()->json();

        $row = collect($json['quizzes'])->firstWhere('content_id', $quizId);
        $this->assertNotNull($row, 'Picker-də iç-içə quiz görünməlidir.');
        $this->assertEquals(['Fənn Testləri', 'Riyaziyyat'], $row['folder_path']);
        $this->assertEquals([$fenn->id, $riyaziyyat->id], $row['folder_path_ids']);
    }

    public function test_quiz_picker_is_scoped_to_own_teacher_quizzes(): void
    {
        $this->actingAs($this->otherTeacher);
        $quizId = $this->quizzes()->create($this->otherTeacher->id, [
            'title' => 'Başqasının quizi',
            'description' => null,
        ])->getKey();

        $this->actingAs($this->teacher);
        $json = $this->getJson('/api/v1/quiz-folders/picker')->assertOk()->json();

        $this->assertEmpty(collect($json['quizzes'])->where('content_id', $quizId));
    }
}
