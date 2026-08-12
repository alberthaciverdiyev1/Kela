<?php

namespace App\Application\Attendance;

use App\Domain\Attendance\Attendance;
use App\Domain\Attendance\AttendanceRepository;
use App\Domain\User\User;
use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;

/**
 * Davam (yoklama) əməliyyatları: workspace tələbələrinin gündəlik iştirakı.
 *
 * Statuslar (Attendance sabitləri):
 *   0 = unknown (qeyd yoxdur), 1 = present (gəldi), 2 = absent (gəlmədi),
 *   3 = late (gecikdi), 4 = excused (üzrlü)
 */
class AttendanceService
{
    public function __construct(
        private readonly AttendanceRepository $attendances,
        private readonly WorkspaceRepository $workspaces,
    ) {
    }

    /**
     * Workspace-in davam cədvəli: hər tələbə üçün seçilən tarixdəki status.
     *
     * @return array{talabalar: array<int, array{id:int,name:string}>, statuslar: array<int,int>}
     */
    public function sheet(int $actingUserId, int $workspaceId, string $date): array
    {
        $workspace = $this->assertOwned($actingUserId, $workspaceId);

        $students = $this->workspaces->students($workspace)
            ->map(fn ($s) => [
                'id' => (int) $s->id,
                'name' => trim(($s->first_name ?? '').' '.($s->last_name ?? '')),
            ])
            ->values()
            ->all();

        $statuses = $this->attendances->forDate($workspaceId, $date)
            ->mapWithKeys(fn (Attendance $a) => [(int) $a->student_id => (int) $a->status])
            ->all();

        return [
            'students' => $students,
            'statuses' => $statuses,
        ];
    }

    /**
     * Tarix üçün davam qeydlərini kütləvi yazır (upsert).
     *
     * @param array<int,int> $statuses  şagird id → status
     */
    public function save(int $actingUserId, int $workspaceId, string $date, array $statuses, ?string $note = null): void
    {
        $workspace = $this->assertOwned($actingUserId, $workspaceId);
        $validIds = $this->workspaces->studentIds($workspace);

        foreach ($statuses as $studentId => $status) {
            $studentId = (int) $studentId;
            if (! in_array($studentId, $validIds, true)) {
                continue; // Workspace-ə aid olmayan şagirdi yazma.
            }

            $status = (int) $status;
            if (! array_key_exists($status, Attendance::STATUS_LABELS)) {
                throw new \InvalidArgumentException('Keçərsiz davam statusu.');
            }

            $this->attendances->upsert($workspaceId, $studentId, $date, $status, $note);
        }
    }

    /**
     * Workspace-in aylıq davam cədvəli: şagirdlər + hər gün üçün statuslar.
     *
     * @return array{students: array<int, array{id:int,name:string}>, days: array<string, array<int,int>>}
     */
    public function monthSheet(int $actingUserId, int $workspaceId, string $month): array
    {
        $workspace = $this->assertOwned($actingUserId, $workspaceId);

        $students = $this->workspaces->students($workspace)
            ->map(fn ($s) => [
                'id' => (int) $s->id,
                'name' => trim(($s->first_name ?? '').' '.($s->last_name ?? '')),
            ])
            ->values()
            ->all();

        $days = [];
        foreach ($this->attendances->forMonth($workspaceId, $month) as $a) {
            $days[$a->date->format('Y-m-d')][(int) $a->student_id] = (int) $a->status;
        }

        return [
            'students' => $students,
            'days' => $days,
        ];
    }

    /**
     * Aylıq davam qeydlərini kütləvi yazır (hər gün üçün upsert).
     *
     * @param array<string, array<int,int>> $days  tarix → (şagird id → status)
     */
    public function saveMonth(int $actingUserId, int $workspaceId, string $month, array $days, ?string $note = null): void
    {
        $workspace = $this->assertOwned($actingUserId, $workspaceId);
        $validIds = $this->workspaces->studentIds($workspace);

        foreach ($days as $date => $statuses) {
            self::validDate($date);
            if (! str_starts_with($date, $month)) {
                throw new \InvalidArgumentException('Qeyd ay ilə uyğun deyil.');
            }

            foreach ($statuses as $studentId => $status) {
                $studentId = (int) $studentId;
                if (! in_array($studentId, $validIds, true)) {
                    continue;
                }
                $status = (int) $status;
                if (! array_key_exists($status, Attendance::STATUS_LABELS)) {
                    throw new \InvalidArgumentException('Keçərsiz davam statusu.');
                }

                $this->attendances->upsert($workspaceId, $studentId, $date, $status, $note);
            }
        }
    }

    /** Ay formatını doğrulayır: YYYY-MM. */
    public static function validMonth(string $month): string
    {
        if (! preg_match('/^\d{4}-\d{2}$/', $month)) {
            throw new \InvalidArgumentException('Ay YYYY-MM formatında olmalıdır.');
        }
        [$y, $m] = array_map('intval', explode('-', $month));
        if ($m < 1 || $m > 12) {
            throw new \InvalidArgumentException('Keçərsiz ay.');
        }

        return $month;
    }

    /** Tarix formatını doğrulayır: YYYY-MM-DD. */
    public static function validDate(string $date): string
    {
        if (! preg_match('/^\d{4}-\d{2}-\d{2}$/', $date)) {
            throw new \InvalidArgumentException('Tarix YYYY-MM-DD formatında olmalıdır.');
        }
        [$y, $m, $d] = array_map('intval', explode('-', $date));
        if (! checkdate($m, $d, $y)) {
            throw new \InvalidArgumentException('Keçərsiz tarix.');
        }

        return $date;
    }

    protected function assertOwned(int $actingUserId, int $workspaceId): Workspace
    {
        $workspace = $this->workspaces->find($workspaceId);
        if ($workspace === null) {
            throw new \RuntimeException('Workspace tapılmadı.');
        }

        $user = User::find($actingUserId);
        if ($user?->isAdmin()) {
            return $workspace;
        }
        if ($workspace->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Bu workspace-ə icazəniz yoxdur.');
        }

        return $workspace;
    }
}
