<?php

namespace Tests\Feature;

use App\Application\Student\StudentExport;
use App\Application\Student\StudentService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use App\Domain\User\User;
use App\Jobs\GenerateStudentsJob;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Illuminate\Support\Facades\Queue;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class StudentGenerateTest extends TestCase
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

    private function students(): StudentService
    {
        return app(StudentService::class);
    }

    private function workspaces(): WorkspaceService
    {
        return app(WorkspaceService::class);
    }

    private function runJob(int $count, ?int $workspaceId = null): void
    {
        (new GenerateStudentsJob($this->teacher->id, $count, $workspaceId))
            ->handle(app(StudentService::class), app(WorkspaceService::class));
    }

    // ── Service (generateMany) ─────────────────────────────────────────────

    public function test_generate_many_creates_numbered_students_with_student_role(): void
    {
        $result = $this->students()->generateMany(['count' => 3]);

        $this->assertCount(3, $result['users']);

        $names = $result['users']->pluck('first_name')->all();
        $this->assertEquals(['Şagird 1', 'Şagird 2', 'Şagird 3'], $names);

        foreach ($result['users'] as $student) {
            $this->assertTrue($student->hasRole(User::ROLE_STUDENT));
        }
    }

    public function test_generate_many_produces_unique_emails(): void
    {
        $result = $this->students()->generateMany(['count' => 5]);

        $emails = collect($result['rows'])->pluck('email')->unique();
        $this->assertCount(5, $emails);
        $this->assertEquals('sagird1@kela.az', $result['rows'][0]['email']);
    }

    public function test_generate_many_skips_existing_emails(): void
    {
        // sagird1@kela.az artıq tutulub → avtomatik sagird2@kela.az-dan davam edir.
        User::factory()->create(['email' => 'sagird1@kela.az']);

        $result = $this->students()->generateMany(['count' => 2]);

        $emails = collect($result['rows'])->pluck('email')->all();
        $this->assertEquals(['sagird2@kela.az', 'sagird3@kela.az'], $emails);
    }

    public function test_generate_many_auto_generates_passwords(): void
    {
        $result = $this->students()->generateMany(['count' => 2]);

        foreach ($result['rows'] as $row) {
            $this->assertNotEmpty($row['password']);
        }
    }

    // ── Queue dispatch (HTTP) ──────────────────────────────────────────────

    public function test_generate_http_flow_dispatches_job_and_redirects(): void
    {
        Queue::fake();

        $this->actingAs($this->teacher)
            ->post('/teacher/students/generate', ['count' => 3])
            ->assertRedirect(route('teacher.students.index'))
            ->assertSessionHas('success');

        Queue::assertPushed(GenerateStudentsJob::class, fn ($job) => $job->count === 3 && $job->workspaceId === null);
    }

    public function test_generate_job_creates_students_and_stores_export(): void
    {
        $this->runJob(3);

        $this->assertDatabaseCount('users', 4); // teacher + 3 yeni
        $this->assertCount(3, StudentExport::rows($this->teacher->id));

        $student = User::where('email', 'sagird1@kela.az')->first();
        $this->assertNotNull($student);
        $this->assertTrue($student->hasRole(User::ROLE_STUDENT));
    }

    public function test_export_downloads_csv_with_credentials(): void
    {
        $this->runJob(2);

        $response = $this->actingAs($this->teacher)
            ->get('/teacher/students/export');

        $response->assertOk();
        $response->assertHeader('Content-Type', 'text/csv; charset=UTF-8');

        $content = $response->streamedContent();
        $this->assertStringContainsString('Şagird 1', $content);
        $this->assertStringContainsString('sagird1@kela.az', $content);

        // Endirmə cache-i təmizləyir.
        $this->assertSame([], StudentExport::rows($this->teacher->id));
    }

    public function test_export_without_pending_data_redirects_with_error(): void
    {
        $this->actingAs($this->teacher)
            ->get('/teacher/students/export')
            ->assertRedirect(route('teacher.students.index'))
            ->assertSessionHas('error');
    }

    // ── Workspace flow ─────────────────────────────────────────────────────

    public function test_workspace_generate_dispatches_job_with_workspace(): void
    {
        $workspaceId = $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu', 50.00)->id;

        Queue::fake();

        $this->actingAs($this->teacher)
            ->post("/teacher/workspaces/{$workspaceId}/students/generate", ['count' => 2])
            ->assertRedirect(route('teacher.workspaces.show', $workspaceId))
            ->assertSessionHas('success');

        Queue::assertPushed(GenerateStudentsJob::class, fn ($job) => $job->workspaceId === $workspaceId && $job->count === 2);
    }

    public function test_workspace_generate_job_attaches_students_with_invoice(): void
    {
        $workspaceId = $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu', 50.00)->id;

        $this->runJob(3, $workspaceId);

        $students = $this->workspaces()->studentList($this->teacher->id, $workspaceId);
        $this->assertCount(3, $students);

        // Hər şagird üçün sinifin aylıq qiyməti ilə avtomatik qaimə.
        foreach ($students as $student) {
            $track = StudentPaymentTrack::where('student_id', $student['id'])
                ->where('workspace_id', $workspaceId)
                ->where('month', now()->format('Y-m'))
                ->first();

            $this->assertNotNull($track);
            $this->assertEquals(50.00, (float) $track->total_amount);
        }

        $this->assertCount(3, StudentExport::rows($this->teacher->id));
    }

    // ── Page states ────────────────────────────────────────────────────────

    public function test_students_page_shows_generate_button_and_export_when_ready(): void
    {
        // Hələ export yoxdur — "Toplu Yarat" var, "Excel Yüklə" yoxdur.
        $html = $this->actingAs($this->teacher)->get('/teacher/students')->assertOk()->getContent();
        $this->assertStringContainsString('Toplu Yarat', $html);
        $this->assertStringNotContainsString('Excel Yüklə', $html);

        // Generasiya bitdikdən sonra "Excel Yüklə" görünür.
        $this->runJob(1);
        $html = $this->actingAs($this->teacher)->get('/teacher/students')->assertOk()->getContent();
        $this->assertStringContainsString('Excel Yüklə', $html);
    }

    public function test_students_page_shows_running_badge_while_generating(): void
    {
        StudentExport::markRunning($this->teacher->id);

        $html = $this->actingAs($this->teacher)->get('/teacher/students')->assertOk()->getContent();

        $this->assertStringContainsString('Generasiya davam edir', $html);
    }
}
