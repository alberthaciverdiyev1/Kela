<?php

namespace App\Domain\Attendance;

use Illuminate\Support\Collection;

/**
 * Davam (attendance) məlumat girişi üçün kontrakt.
 */
interface AttendanceRepository
{
    /** Verilən tarixdə workspace-dəki bütün davam qeydləri. */
    public function forDate(int $workspaceId, string $date): Collection;

    /** Verilən ay ərzində (YYYY-MM) workspace-dəki bütün davam qeydləri. */
    public function forMonth(int $workspaceId, string $month): Collection;

    /** Tarix + şagird üçün davam qeydini əlavə/yenilə (upsert). */
    public function upsert(int $workspaceId, int $studentId, string $date, int $status, ?string $note = null): Attendance;
}
