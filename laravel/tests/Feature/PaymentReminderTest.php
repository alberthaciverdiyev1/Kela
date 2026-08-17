<?php

namespace Tests\Feature;

use App\Application\Payment\PaymentService;
use App\Application\PaymentReminder\PaymentReminderService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\PaymentReminder\Enums\PaymentReminderType;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use App\Domain\User\User;
use Illuminate\Foundation\Testing\RefreshDatabase;
use Spatie\Permission\Models\Role;
use Tests\TestCase;

class PaymentReminderTest extends TestCase
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

    private function reminders(): PaymentReminderService
    {
        return app(PaymentReminderService::class);
    }

    private function payments(): PaymentService
    {
        return app(PaymentService::class);
    }

    private function workspaces(): WorkspaceService
    {
        return app(WorkspaceService::class);
    }

    private function makeStudent(string $name = 'Borc'): User
    {
        $student = User::factory()->create(['first_name' => $name]);
        $student->assignRole(User::ROLE_STUDENT);

        return $student;
    }

    private function makeTrack(): StudentPaymentTrack
    {
        $workspaceId = $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu', 50.00)->id;
        $student = $this->makeStudent();
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);

        return $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, now()->format('Y-m'));
    }

    public function test_upcoming_reminder_sent_five_days_before_due(): void
    {
        $track = $this->makeTrack();
        $track->update(['due_date' => now()->addDays(5)]);

        $sent = $this->reminders()->run();

        $this->assertEquals(1, $sent);
        $this->assertDatabaseHas('payment_reminders', [
            'payment_track_id' => $track->id,
            'type' => PaymentReminderType::UPCOMING->value,
        ]);
    }

    public function test_reminders_are_idempotent(): void
    {
        $track = $this->makeTrack();
        $track->update(['due_date' => now()->addDays(5)]);

        $this->assertEquals(1, $this->reminders()->run());
        $this->assertEquals(0, $this->reminders()->run());
        $this->assertEquals(0, $this->reminders()->run());

        $this->assertDatabaseCount('payment_reminders', 1);
    }

    public function test_due_reminder_sent_on_due_day(): void
    {
        $track = $this->makeTrack();
        $track->update(['due_date' => now()]);

        $sent = $this->reminders()->run();

        $this->assertEquals(1, $sent);
        $this->assertDatabaseHas('payment_reminders', [
            'payment_track_id' => $track->id,
            'type' => PaymentReminderType::DUE->value,
        ]);
    }

    public function test_both_reminders_sequence_upcoming_then_due(): void
    {
        $track = $this->makeTrack();

        // 5 gün qalmış → upcoming
        $track->update(['due_date' => now()->addDays(5)]);
        $this->assertEquals(1, $this->reminders()->run());

        // Ödəniş günü → due
        $track->update(['due_date' => now()]);
        $this->assertEquals(1, $this->reminders()->run());

        $this->assertDatabaseCount('payment_reminders', 2);
        $this->assertDatabaseHas('payment_reminders', ['payment_track_id' => $track->id, 'type' => 'upcoming']);
        $this->assertDatabaseHas('payment_reminders', ['payment_track_id' => $track->id, 'type' => 'due']);
    }

    public function test_no_reminder_for_paid_track(): void
    {
        $track = $this->makeTrack();
        $track->update(['due_date' => now()]);

        // Tam ödə
        $this->payments()->acceptPayment($this->teacher->id, $track->id, 50.00);

        $sent = $this->reminders()->run();

        $this->assertEquals(0, $sent);
        $this->assertDatabaseCount('payment_reminders', 0);
    }

    public function test_no_reminder_far_in_future(): void
    {
        $track = $this->makeTrack();
        $track->update(['due_date' => now()->addDays(10)]);

        $sent = $this->reminders()->run();

        $this->assertEquals(0, $sent);
        $this->assertDatabaseCount('payment_reminders', 0);
    }

    public function test_reminders_page_lists_teacher_reminders(): void
    {
        $workspaceId = $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu', 50.00)->id;
        $student = $this->makeStudent('Borcçu');
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, now()->format('Y-m'));
        $track->update(['due_date' => now()->addDays(5)]);
        $this->reminders()->run();

        $html = $this->actingAs($this->teacher)
            ->get('/teacher/payments/reminders')
            ->assertOk()
            ->getContent();

        $this->assertStringContainsString('Ödəniş Bildirişləri', $html);
        $this->assertStringContainsString('Borcçu', $html);
        $this->assertStringContainsString('gün qalıb', $html);
    }

    public function test_reminders_page_hides_other_teachers_reminders(): void
    {
        $workspaceId = $this->workspaces()->create($this->teacher->id, 'Sınaq Qrupu', 50.00)->id;
        $student = $this->makeStudent('Başqası');
        $this->workspaces()->attachStudents($this->teacher->id, $workspaceId, [$student->id]);
        $track = $this->payments()->generateInvoice($this->teacher->id, $student->id, $workspaceId, now()->format('Y-m'));
        $track->update(['due_date' => now()->addDays(5)]);
        $this->reminders()->run();

        $html = $this->actingAs($this->otherTeacher)
            ->get('/teacher/payments/reminders')
            ->assertOk()
            ->getContent();

        $this->assertStringContainsString('Bildiriş yoxdur', $html);
        $this->assertStringNotContainsString('Başqası', $html);
    }

    public function test_payments_page_links_to_reminders(): void
    {
        $this->actingAs($this->teacher)
            ->get('/teacher/payments')
            ->assertOk()
            ->assertSee(route('teacher.payments.reminders'));
    }
}
