<?php

namespace Tests\Feature;

use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

/**
 * Backend API (JSON) — v1. Frontend server-rendered Blade-dən tamamilə ayrıdır.
 * Bütün endpointlər Application servisləri üzərindən işləyir.
 */
class ApiTest extends TestCase
{
    use RefreshDatabase;

    protected User $teacher;
    protected User $student;

    protected function setUp(): void
    {
        parent::setUp();

        foreach (User::ALL_ROLES as $role) {
            Role::firstOrCreate(['name' => $role]);
        }

        $this->teacher = User::factory()->create();
        $this->teacher->assignRole(User::ROLE_TEACHER);

        $this->student = User::factory()->create();
        $this->student->assignRole(User::ROLE_STUDENT);
    }

    // --- Auth ---

    public function test_login_returns_bearer_token(): void
    {
        $response = $this->postJson('/api/v1/auth/login', [
            'email' => $this->teacher->email,
            'password' => 'password',
        ]);

        $response->assertOk()
            ->assertJsonStructure([
                'data' => ['token', 'token_type', 'user' => ['id', 'full_name', 'email', 'roles']],
            ])
            ->assertJsonPath('data.token_type', 'Bearer')
            ->assertJsonPath('data.user.id', $this->teacher->id);
    }

    public function test_login_with_wrong_credentials_returns_422(): void
    {
        $this->postJson('/api/v1/auth/login', [
            'email' => $this->teacher->email,
            'password' => 'wrong-password',
        ])->assertStatus(422);
    }

    public function test_me_returns_current_user(): void
    {
        Sanctum::actingAs($this->teacher);

        $this->getJson('/api/v1/auth/me')
            ->assertOk()
            ->assertJsonPath('data.id', $this->teacher->id)
            ->assertJsonPath('data.roles', [User::ROLE_TEACHER]);
    }

    public function test_unauthenticated_api_returns_401(): void
    {
        $this->getJson('/api/v1/students')->assertStatus(401);
    }

    public function test_student_role_is_forbidden_from_admin_resources(): void
    {
        Sanctum::actingAs($this->student);

        $this->getJson('/api/v1/lessons')->assertStatus(403);
        $this->postJson('/api/v1/students', ['first_name' => 'X'])->assertStatus(403);
    }

    // --- Cities ---

    public function test_cities_available_to_any_authenticated_user(): void
    {
        Sanctum::actingAs($this->student);

        $this->getJson('/api/v1/cities')->assertOk()->assertJsonStructure(['data']);
    }

    // --- Students ---

    public function test_student_crud_via_api(): void
    {
        Sanctum::actingAs($this->teacher);

        // create
        $created = $this->postJson('/api/v1/students', [
            'first_name' => 'Ali',
            'last_name' => 'Vəliyev',
            'email' => 'ali@test.az',
            'password' => 'secret123',
            'status' => 1,
        ])->assertStatus(201)->assertJsonPath('data.full_name', 'Ali Vəliyev')->json('data');

        // index — setUp-dakı şagird + yenisi
        $students = $this->getJson('/api/v1/students')->assertOk()->json('data');
        $this->assertTrue(collect($students)->contains('email', 'ali@test.az'));

        // show
        $this->getJson("/api/v1/students/{$created['id']}")
            ->assertOk()
            ->assertJsonPath('data.email', 'ali@test.az');

        // update
        $this->putJson("/api/v1/students/{$created['id']}", [
            'first_name' => 'Ali',
            'last_name' => 'Vəliyev',
            'email' => 'ali2@test.az',
        ])->assertOk()->assertJsonPath('data.email', 'ali2@test.az');

        // delete (soft delete)
        $this->deleteJson("/api/v1/students/{$created['id']}")->assertOk();
        $this->assertSoftDeleted('users', ['id' => $created['id']]);
    }

    public function test_show_non_student_user_returns_404(): void
    {
        Sanctum::actingAs($this->teacher);

        $this->getJson("/api/v1/students/{$this->teacher->id}")->assertStatus(404);
    }

    // --- Lessons ---

    public function test_lesson_create_and_list_via_api(): void
    {
        Sanctum::actingAs($this->teacher);

        $this->postJson('/api/v1/lessons', [
            'title' => 'Riyaziyyat',
            'description' => 'Giriş',
            'is_published' => true,
            'order_index' => 1,
        ])->assertStatus(201)->assertJsonPath('data.title', 'Riyaziyyat');

        $this->getJson('/api/v1/lessons')
            ->assertOk()
            ->assertJsonCount(1, 'data')
            ->assertJsonPath('data.0.title', 'Riyaziyyat')
            ->assertJsonPath('data.0.is_published', true);
    }

    public function test_lesson_show_returns_viewer_urls(): void
    {
        Sanctum::actingAs($this->teacher);

        $lessonId = app(\App\Application\Lesson\LessonService::class)
            ->create($this->teacher->id, ['title' => 'Video', 'is_published' => true, 'video_path' => 'videos/x.mp4'])
            ->content_id;

        $this->getJson("/api/v1/lessons/{$lessonId}")
            ->assertOk()
            ->assertJsonStructure(['data', 'viewer' => ['has_video', 'stream_url', 'thumbnail_url']]);
    }

    public function test_lesson_access_control_for_other_teacher(): void
    {
        $other = User::factory()->create();
        $other->assignRole(User::ROLE_TEACHER);
        $lessonId = app(\App\Application\Lesson\LessonService::class)
            ->create($this->teacher->id, ['title' => 'X', 'is_published' => true])
            ->content_id;

        Sanctum::actingAs($other);

        $this->getJson("/api/v1/lessons/{$lessonId}")->assertStatus(403);
        $this->deleteJson("/api/v1/lessons/{$lessonId}")->assertStatus(403);
    }

    // --- Quizzes + Questions ---

    public function test_quiz_question_flow_via_api(): void
    {
        Sanctum::actingAs($this->teacher);

        $quiz = $this->postJson('/api/v1/quizzes', [
            'title' => 'Sınaq',
            'description' => 'Fənn testi',
        ])->assertStatus(201)->json('data');

        $question = $this->postJson('/api/v1/questions', [
            'text' => '2+2=?',
            'option_a' => '3',
            'option_b' => '4',
            'correct_option' => 1,
        ])->assertStatus(201)->json('data');

        // add question to quiz
        $this->postJson("/api/v1/quizzes/{$quiz['content_id']}/questions", [
            'question_id' => $question['id'],
        ])->assertOk();

        $this->getJson("/api/v1/quizzes/{$quiz['content_id']}")
            ->assertOk()
            ->assertJsonCount(1, 'questions')
            ->assertJsonPath('questions.0.text', '2+2=?');

        // move up/down and remove
        $this->postJson("/api/v1/quizzes/{$quiz['content_id']}/questions/{$question['id']}/move", [
            'direction' => 'down',
        ])->assertOk();
        $this->deleteJson("/api/v1/quizzes/{$quiz['content_id']}/questions/{$question['id']}")->assertOk();
        $this->getJson("/api/v1/quizzes/{$quiz['content_id']}")->assertJsonCount(0, 'questions');
    }

    // --- Workspaces ---

    public function test_workspace_flow_via_api(): void
    {
        Sanctum::actingAs($this->teacher);

        $ws = $this->postJson('/api/v1/workspaces', ['name' => 'Sinif 3A'])
            ->assertStatus(201)->json('data');

        // add a lesson content to workspace
        $lessonId = app(\App\Application\Lesson\LessonService::class)
            ->create($this->teacher->id, ['title' => 'Riyaziyyat', 'is_published' => true])
            ->content_id;

        $this->postJson("/api/v1/workspaces/{$ws['id']}/contents", [
            'content_id' => $lessonId,
        ])->assertStatus(201);

        // show → directory contains it
        $this->getJson("/api/v1/workspaces/{$ws['id']}")
            ->assertOk()
            ->assertJsonStructure(['data', 'students', 'directory'])
            ->assertJsonPath('data.name', 'Sinif 3A')
            ->assertJsonCount(1, 'directory.contents');

        // attach student
        $this->postJson("/api/v1/workspaces/{$ws['id']}/students", [
            'student_ids' => [$this->student->id],
        ])->assertOk();

        $this->getJson("/api/v1/workspaces/{$ws['id']}")
            ->assertJsonCount(1, 'students');

        // detach
        $this->deleteJson("/api/v1/workspaces/{$ws['id']}/students/{$this->student->id}")->assertOk();

        // delete workspace (soft delete)
        $this->deleteJson("/api/v1/workspaces/{$ws['id']}")->assertOk();
        $this->assertSoftDeleted('workspaces', ['id' => $ws['id']]);
    }
}
