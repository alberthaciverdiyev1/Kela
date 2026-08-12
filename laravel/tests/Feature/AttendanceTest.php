<?php

namespace Tests\Feature;

use App\Application\Attendance\AttendanceService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\Attendance\Attendance;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Laravel\Sanctum\Sanctum;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class AttendanceTest extends TestCase
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

    private function attendances(): AttendanceService
    {
        return app(AttendanceService::class);
    }

    private function makeStudent(string $firstName = 'Ali'): User
    {
        $student = User::factory()->create(['first_name' => $firstName]);
        $student->assignRole(User::ROLE_STUDENT);

        return $student;
    }

    private function makeWorkspaceWithStudents(int $studentCount = 2): array
    {
        $workspaceId = $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu')->id;
        $students = collect();
        for ($i = 0; $i < $studentCount; $i++) {
            $students->push($this->makeStudent('Şagird '.($i + 1)));
        }
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, $students->pluck('id')->all());

        return [$workspaceId, $students];
    }

    public function test_attendance_page_renders_month_sheet_ui(): void
    {
        [$workspaceId, $students] = $this->makeWorkspaceWithStudents(2);

        $this->actingAs($this->teacher);

        $html = $this->get('/teacher/attendance?workspace='.$workspaceId.'&month=2026-08')->assertOk()->getContent();

        // Navbar linki və səhifə mövcuddur
        $this->assertStringContainsString('Davam', $html);
        $this->assertStringContainsString('x-data="attendanceMonth', $html);
        $this->assertStringContainsString('Workspace seç', $html);
        $this->assertStringContainsString('Sınaq Qrupu', $html);
        $this->assertStringContainsString('2026-08', $html);

        // Popover + avtomatik kayıt interaksiyonu (ayrıca Saxla düyməsi yoxdur)
        $this->assertStringContainsString('openOptions', $html);
        $this->assertStringContainsString('selectStatus', $html);
        $this->assertStringContainsString('saveState', $html);
        $this->assertStringNotContainsString('@click="save()"', $html);
    }

    public function test_attendance_page_is_not_in_workspace_show(): void
    {
        [$workspaceId] = $this->makeWorkspaceWithStudents(1);

        $this->actingAs($this->teacher);

        $html = $this->get("/teacher/workspaces/{$workspaceId}")->assertOk()->getContent();
        $this->assertStringNotContainsString('setAttendanceStatus', $html);
        $this->assertStringNotContainsString('attendanceDate', $html);
    }

    public function test_api_returns_month_sheet_grouped_by_date(): void
    {
        [$workspaceId, $students] = $this->makeWorkspaceWithStudents(2);

        $this->attendances()->saveMonth($this->teacher->id, $workspaceId, '2026-08', [
            '2026-08-03' => [$students[0]->id => Attendance::STATUS_PRESENT],
            '2026-08-17' => [$students[1]->id => Attendance::STATUS_ABSENT],
        ]);

        Sanctum::actingAs($this->teacher);

        $res = $this->getJson("/api/v1/workspaces/{$workspaceId}/attendance/month?month=2026-08")->assertOk()->json('data');
        $this->assertCount(2, $res['students']);
        $this->assertEquals(Attendance::STATUS_PRESENT, $res['days']['2026-08-03'][$students[0]->id]);
        $this->assertEquals(Attendance::STATUS_ABSENT, $res['days']['2026-08-17'][$students[1]->id]);
        $this->assertArrayNotHasKey('2026-08-04', $res['days']);
    }

    public function test_api_saves_month_statuses_and_rejects_out_of_month(): void
    {
        [$workspaceId, $students] = $this->makeWorkspaceWithStudents(2);

        Sanctum::actingAs($this->teacher);

        // Ay xaricindəki gün qəbul edilmir.
        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance/month", [
            'month' => '2026-08',
            'days' => ['2026-09-01' => [(string) $students[0]->id => Attendance::STATUS_PRESENT]],
        ])->assertStatus(422);

        // Keçərli aylıq qeyd.
        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance/month", [
            'month' => '2026-08',
            'days' => [
                '2026-08-03' => [(string) $students[0]->id => Attendance::STATUS_LATE, (string) $students[1]->id => Attendance::STATUS_EXCUSED],
                '2026-08-04' => [(string) $students[0]->id => Attendance::STATUS_PRESENT],
            ],
        ])->assertOk();

        $sheet = $this->attendances()->monthSheet($this->teacher->id, $workspaceId, '2026-08');
        $this->assertEquals(Attendance::STATUS_LATE, $sheet['days']['2026-08-03'][$students[0]->id]);
        $this->assertEquals(Attendance::STATUS_EXCUSED, $sheet['days']['2026-08-03'][$students[1]->id]);
        $this->assertEquals(Attendance::STATUS_PRESENT, $sheet['days']['2026-08-04'][$students[0]->id]);

        // Keçərsiz ay formatı.
        $this->getJson("/api/v1/workspaces/{$workspaceId}/attendance/month?month=2026-13")->assertStatus(422);
        $this->getJson("/api/v1/workspaces/{$workspaceId}/attendance/month?month=bad")->assertStatus(422);
    }

    public function test_month_ownership_prevents_cross_teacher_access(): void
    {
        [$workspaceId] = $this->makeWorkspaceWithStudents(1);

        Sanctum::actingAs($this->otherTeacher);

        $this->getJson("/api/v1/workspaces/{$workspaceId}/attendance/month?month=2026-08")->assertStatus(403);
        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance/month", [
            'month' => '2026-08',
            'days' => [],
        ])->assertStatus(403);
    }

    public function test_api_month_ignores_students_not_in_workspace(): void
    {
        [$workspaceId] = $this->makeWorkspaceWithStudents(1);
        $outsider = $this->makeStudent('Kənar');

        Sanctum::actingAs($this->teacher);

        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance/month", [
            'month' => '2026-08',
            'days' => ['2026-08-01' => [(string) $outsider->id => Attendance::STATUS_PRESENT]],
        ])->assertOk();

        $this->assertDatabaseCount('attendances', 0);
    }

    public function test_api_returns_attendance_sheet_for_date(): void
    {
        [$workspaceId, $students] = $this->makeWorkspaceWithStudents(2);

        $this->attendances()->save($this->teacher->id, $workspaceId, '2026-08-12', [
            $students[0]->id => Attendance::STATUS_PRESENT,
            $students[1]->id => Attendance::STATUS_ABSENT,
        ]);

        Sanctum::actingAs($this->teacher);

        $res = $this->getJson("/api/v1/workspaces/{$workspaceId}/attendance?date=2026-08-12")->assertOk()->json('data');
        $this->assertCount(2, $res['students']);
        $this->assertEquals(Attendance::STATUS_PRESENT, $res['statuses'][$students[0]->id]);
        $this->assertEquals(Attendance::STATUS_ABSENT, $res['statuses'][$students[1]->id]);
    }

    public function test_api_saves_attendance_statuses(): void
    {
        [$workspaceId, $students] = $this->makeWorkspaceWithStudents(2);

        Sanctum::actingAs($this->teacher);

        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance", [
            'date' => '2026-08-12',
            'statuses' => [
                (string) $students[0]->id => Attendance::STATUS_LATE,
                (string) $students[1]->id => Attendance::STATUS_EXCUSED,
            ],
        ])->assertOk();

        // Upsert: ikinci yazım birincini yeniləyir (kiçik doğrulama).
        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance", [
            'date' => '2026-08-12',
            'statuses' => [(string) $students[0]->id => Attendance::STATUS_ABSENT],
        ])->assertOk();

        $sheet = $this->attendances()->sheet($this->teacher->id, $workspaceId, '2026-08-12');
        $this->assertEquals(Attendance::STATUS_ABSENT, $sheet['statuses'][$students[0]->id]);
        $this->assertEquals(Attendance::STATUS_EXCUSED, $sheet['statuses'][$students[1]->id]);

        // DB-də yalnız 2 qeyd var.
        $this->assertDatabaseCount('attendances', 2);
    }

    public function test_api_attendance_rejects_invalid_status_and_date(): void
    {
        [$workspaceId, $students] = $this->makeWorkspaceWithStudents(1);

        Sanctum::actingAs($this->teacher);

        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance", [
            'date' => '2026-08-12',
            'statuses' => [(string) $students[0]->id => 99],
        ])->assertStatus(422);

        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance", [
            'date' => 'invalid',
            'statuses' => [(string) $students[0]->id => Attendance::STATUS_PRESENT],
        ])->assertStatus(422);

        $this->getJson("/api/v1/workspaces/{$workspaceId}/attendance?date=bad")->assertStatus(422);
    }

    public function test_attendance_ownership_prevents_cross_teacher_access(): void
    {
        [$workspaceId, $students] = $this->makeWorkspaceWithStudents(1);

        Sanctum::actingAs($this->otherTeacher);

        $this->getJson("/api/v1/workspaces/{$workspaceId}/attendance?date=2026-08-12")->assertStatus(403);
        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance", [
            'date' => '2026-08-12',
            'statuses' => [(string) $students[0]->id => Attendance::STATUS_PRESENT],
        ])->assertStatus(403);
    }

    public function test_api_ignores_students_not_in_workspace(): void
    {
        [$workspaceId] = $this->makeWorkspaceWithStudents(1);
        $outsider = $this->makeStudent('Kənar');

        Sanctum::actingAs($this->teacher);

        $this->postJson("/api/v1/workspaces/{$workspaceId}/attendance", [
            'date' => '2026-08-12',
            'statuses' => [(string) $outsider->id => Attendance::STATUS_PRESENT],
        ])->assertOk();

        // Kənar şagird üçün qeyd yazılmadı.
        $this->assertDatabaseCount('attendances', 0);
    }
}
