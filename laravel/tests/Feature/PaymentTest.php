<?php

namespace Tests\Feature;

use App\Application\Payment\PaymentService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\StudentPaymentTrack\Enums\PaymentStatus;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class PaymentTest extends TestCase
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

    private function payments(): PaymentService
    {
        return app(PaymentService::class);
    }

    private function workspaces(): WorkspaceService
    {
        return app(WorkspaceService::class);
    }

    private function makeStudent(string $name = 'Ali'): User
    {
        $student = User::factory()->create(['first_name' => $name]);
        $student->assignRole(User::ROLE_STUDENT);

        return $student;
    }

    private function makeWorkspace(?float $monthlyPrice = null): int
    {
        return $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu', $monthlyPrice)->id;
    }

    public function test_invoice_uses_workspace_monthly_price(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);

        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->assertNotNull($track);
        $this->assertEquals(50.00, (float) $track->total_amount);
        $this->assertEquals(PaymentStatus::PENDING->value, $track->status);
        $this->assertEquals('2026-08', $track->month);
    }

    public function test_agreed_price_overrides_monthly_price(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id], 40.00, '2026-08-01');

        // Fərqli ay üçün qaimə — agreed_price pivot-dan gəlir.
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-09');

        $this->assertEquals(40.00, (float) $track->total_amount);
    }

    public function test_explicit_amount_overrides_all_prices(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);

        // Cari ay üçün avtomatik yaranan qaimə ilə toqquşmamaq üçün növbəti ayı istifadə edirik.
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-09', 75.00);

        $this->assertEquals(75.00, (float) $track->total_amount);
    }

    public function test_duplicate_invoice_returns_existing(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);

        $first = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');
        $second = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->assertSame($first->id, $second->id);
        $this->assertDatabaseCount('student_payment_tracks', 1);
    }

    public function test_student_not_in_workspace_is_rejected(): void
    {
        $workspaceId = $this->makeWorkspace();
        $outsider = $this->makeStudent();

        $this->expectException(\RuntimeException::class);
        $this->payments()->generateInvoice($this->teacher->id, $outsider->id, $workspaceId, '2026-08');
    }

    public function test_other_teacher_cannot_generate_invoice(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);

        $this->expectException(\RuntimeException::class);
        $this->payments()->generateInvoice($this->otherTeacher->id, $student->id, $workspaceId, '2026-08');
    }

    public function test_other_teacher_cannot_accept_payment(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->expectException(\RuntimeException::class);
        $this->payments()->acceptPayment($this->otherTeacher->id, $track->id, 20.00);
    }

    public function test_other_teacher_cannot_update_track(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->expectException(\RuntimeException::class);
        $this->payments()->updateTotalAmount($this->otherTeacher->id, $track->id, 60.00);
    }

    public function test_status_transitions_pending_partial_paid(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->assertEquals(PaymentStatus::PENDING->value, $track->status);

        // Qismən ödəniş → PARTIAL
        $this->payments()->acceptPayment($this->teacher->id, $track->id, 20.00);
        $track->refresh();
        $this->assertEquals(PaymentStatus::PARTIAL->value, $track->status);
        $this->assertEquals(20.00, (float) $track->paid_amount);

        // Qalan borc → PAID
        $this->payments()->acceptPayment($this->teacher->id, $track->id, 30.00);
        $track->refresh();
        $this->assertEquals(PaymentStatus::PAID->value, $track->status);
        $this->assertEquals(50.00, (float) $track->paid_amount);

        // 2 transaksiya yaranıb
        $this->assertDatabaseCount('student_payment_transactions', 2);
    }

    public function test_overpayment_is_rejected(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->expectException(\InvalidArgumentException::class);
        $this->payments()->acceptPayment($this->teacher->id, $track->id, 60.00);
    }

    public function test_total_amount_cannot_go_below_paid(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->payments()->acceptPayment($this->teacher->id, $track->id, 30.00);

        $this->expectException(\InvalidArgumentException::class);
        $this->payments()->updateTotalAmount($this->teacher->id, $track->id, 20.00);
    }

    public function test_mark_overdue_flags_expired_tracks(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);

        // Cari ay üçün track — due_date ay sonudur (hələ keçməyib) → OVERDUE olmamalı.
        $current = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, now()->format('Y-m'));

        $this->assertEquals(0, $this->payments()->markOverdueForTeacher($this->teacher->id));

        // due_date-i keçmişə çək → OVERDUE.
        $current->update(['due_date' => now()->subDay()]);

        $count = $this->payments()->markOverdueForTeacher($this->teacher->id);
        $this->assertEquals(1, $count);

        $current->refresh();
        $this->assertEquals(PaymentStatus::OVERDUE->value, $current->status);
    }

    public function test_invoice_prorated_by_start_date(): void
    {
        $workspaceId = $this->makeWorkspace(30.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id], null, '2026-08-15');

        // Avqustun 15-də qoşulub → 17 gün qalır (15..31) → 30 * 17/31 ≈ 16.45
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->assertNotNull($track);
        $this->assertEqualsWithDelta(30.00 * 17 / 31, (float) $track->total_amount, 0.02);
    }

    public function test_invoice_not_generated_before_start_date(): void
    {
        $workspaceId = $this->makeWorkspace(30.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id], null, '2026-09-01');

        // Avqust üçün qaimə yoxdur (şagird hələ qoşulmayıb).
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, '2026-08');

        $this->assertNull($track);
    }

    public function test_payments_page_renders(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, now()->format('Y-m'));

        $this->actingAs($this->teacher)
            ->get('/teacher/payments?workspace='.$workspaceId)
            ->assertOk()
            ->assertSee('Ödənişlər')
            ->assertSee($student->full_name);
    }

    public function test_store_payment_http_flow(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, now()->format('Y-m'));

        $this->actingAs($this->teacher)
            ->post('/teacher/payments', [
                'track_id' => $track->id,
                'amount' => 50.00,
            ])
            ->assertRedirect()
            ->assertSessionHas('success');

        $track->refresh();
        $this->assertEquals(PaymentStatus::PAID->value, $track->status);
    }

    public function test_generate_invoice_http_flow(): void
    {
        $workspaceId = $this->makeWorkspace(50.00);
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);

        $this->actingAs($this->teacher)
            ->post('/teacher/payments/generate', [
                'student_id' => $student->id,
                'workspace_id' => $workspaceId,
                'month' => now()->format('Y-m'),
            ])
            ->assertRedirect()
            ->assertSessionHas('success');

        $this->assertDatabaseHas('student_payment_tracks', [
            'student_id' => $student->id,
            'workspace_id' => $workspaceId,
            'month' => now()->format('Y-m'),
        ]);
    }
}
