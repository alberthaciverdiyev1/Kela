<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use App\Domain\StudentPaymentTrack\StudentPaymentTransaction;
use App\Domain\StudentPaymentTrack\StudentPaymentTrackRepository;
use Illuminate\Support\Collection;

class EloquentStudentPaymentTrackRepository implements StudentPaymentTrackRepository
{
    public function getTracksForTeacher(int $teacherId, ?int $workspaceId, string $month): Collection
    {
        $query = StudentPaymentTrack::with(['student', 'workspace', 'transactions'])
            ->whereHas('workspace', function ($q) use ($teacherId) {
                $q->where('teacher_id', $teacherId);
            })
            ->where('month', $month);

        if ($workspaceId) {
            $query->where('workspace_id', $workspaceId);
        }

        return $query->get();
    }

    public function find(int $id): ?StudentPaymentTrack
    {
        return StudentPaymentTrack::find($id);
    }

    public function findByStudentWorkspaceMonth(int $studentId, int $workspaceId, string $month): ?StudentPaymentTrack
    {
        return StudentPaymentTrack::where('student_id', $studentId)
            ->where('workspace_id', $workspaceId)
            ->where('month', $month)
            ->first();
    }

    public function save(StudentPaymentTrack $track): bool
    {
        return $track->save();
    }

    public function createTransaction(int $trackId, float $amount, ?string $note = null): StudentPaymentTransaction
    {
        return StudentPaymentTransaction::create([
            'payment_track_id' => $trackId,
            'amount' => $amount,
            'note' => $note,
        ]);
    }

    public function markOverdueForTeacher(int $teacherId): int
    {
        return StudentPaymentTrack::query()
            ->whereHas('workspace', function ($q) use ($teacherId) {
                $q->where('teacher_id', $teacherId);
            })
            ->where('due_date', '<', now())
            ->whereIn('status', [
                StudentPaymentTrack::STATUS_PENDING,
                StudentPaymentTrack::STATUS_PARTIAL,
            ])
            ->update(['status' => StudentPaymentTrack::STATUS_OVERDUE]);
    }
}
