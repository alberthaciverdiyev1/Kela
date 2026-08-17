<?php

namespace App\Domain\StudentPaymentTrack;

use Illuminate\Support\Collection;

interface StudentPaymentTrackRepository
{
    /**
     * @param int $teacherId
     * @param int|null $workspaceId
     * @param string $month YYYY-MM
     * @return Collection
     */
    public function getTracksForTeacher(int $teacherId, ?int $workspaceId, string $month): Collection;

    public function find(int $id): ?StudentPaymentTrack;
    
    public function findByStudentWorkspaceMonth(int $studentId, int $workspaceId, string $month): ?StudentPaymentTrack;
    
    public function save(StudentPaymentTrack $track): bool;

    public function createTransaction(int $trackId, float $amount, ?string $note = null): StudentPaymentTransaction;

    /**
     * Müddəti ötmüş (due_date < now) və hələ ödənilməmiş/qismən ödənilmiş
     * track-ları OVERDUE olaraq işarələyir. Teacher-a aid olanları.
     *
     * @return int yenilənmiş track sayı
     */
    public function markOverdueForTeacher(int $teacherId): int;
}
