<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Attendance\Attendance;
use App\Domain\Attendance\AttendanceRepository;
use Illuminate\Support\Collection;

class EloquentAttendanceRepository implements AttendanceRepository
{
    public function forDate(int $workspaceId, string $date): Collection
    {
        return Attendance::query()
            ->where('workspace_id', $workspaceId)
            ->where('date', $date)
            ->get(['student_id', 'status', 'note']);
    }

    public function forMonth(int $workspaceId, string $month): Collection
    {
        [$y, $m] = array_map('intval', explode('-', $month));
        $first = sprintf('%04d-%02d-01', $y, $m);
        $last = (new \DateTimeImmutable($first))->modify('last day of this month')->format('Y-m-d');

        return Attendance::query()
            ->where('workspace_id', $workspaceId)
            ->whereBetween('date', [$first, $last])
            ->get(['student_id', 'date', 'status']);
    }

    public function upsert(int $workspaceId, int $studentId, string $date, int $status, ?string $note = null): Attendance
    {
        return Attendance::query()->updateOrCreate(
            [
                'workspace_id' => $workspaceId,
                'student_id' => $studentId,
                'date' => $date,
            ],
            [
                'status' => $status,
                'note' => $note,
            ]
        );
    }
}
