<?php

namespace App\Application\Payment;

use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use App\Domain\StudentPaymentTrack\StudentPaymentTrackRepository;
use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
use Illuminate\Support\Collection;

class PaymentService
{
    public function __construct(
        private readonly StudentPaymentTrackRepository $payments,
        private readonly WorkspaceRepository $workspaces,
    ) {
    }

    /**
     * @return Collection<StudentPaymentTrack>
     */
    public function getMonthlySheet(int $teacherId, ?int $workspaceId, string $month): Collection
    {
        return $this->payments->getTracksForTeacher($teacherId, $workspaceId, $month);
    }

    /**
     * Tələbə üçün qaimə (track) yaradır, əgər yoxdursa.
     */
    public function generateInvoice(int $studentId, int $workspaceId, string $month, ?float $amount = null): StudentPaymentTrack
    {
        $existing = $this->payments->findByStudentWorkspaceMonth($studentId, $workspaceId, $month);
        if ($existing) {
            return $existing;
        }

        /** @var Workspace $workspace */
        $workspace = $this->workspaces->find($workspaceId);
        if (!$workspace) {
            throw new \RuntimeException('Workspace not found');
        }

        // Tələbə üçün xüsusi razılaşdırılmış qiymət varmı? (Pivot)
        $studentPivot = $workspace->students()->where('users.id', $studentId)->first();
        if (!$studentPivot) {
            throw new \RuntimeException('Student is not in this workspace');
        }

        $price = $amount ?? $studentPivot->pivot->agreed_price ?? $workspace->monthly_price ?? 0.0;

        $track = new StudentPaymentTrack([
            'student_id' => $studentId,
            'workspace_id' => $workspaceId,
            'month' => $month,
            'total_amount' => $price,
            'paid_amount' => 0.0,
            'status' => StudentPaymentTrack::STATUS_PENDING,
            'due_date' => \Carbon\Carbon::createFromFormat('Y-m', $month)->endOfMonth(),
        ]);

        $this->payments->save($track);
        return $track;
    }

    /**
     * Tələbədən ödəniş almaq.
     */
    public function acceptPayment(int $trackId, float $amount, ?string $note = null): void
    {
        $track = $this->payments->find($trackId);
        if (!$track) {
            throw new \RuntimeException('Payment track not found');
        }

        if ($amount <= 0) {
            throw new \InvalidArgumentException('Məbləğ sıfırdan böyük olmalıdır.');
        }

        $this->payments->createTransaction($trackId, $amount, $note);

        $track->paid_amount += $amount;

        if ($track->paid_amount >= $track->total_amount) {
            $track->status = StudentPaymentTrack::STATUS_PAID;
        } else {
            $track->status = 1; // Partial
        }

        $this->payments->save($track);
    }

    /**
     * Tələbənin ödəniş qaiməsindəki ümumi borc məbləğini yeniləmək.
     */
    public function updateTotalAmount(int $trackId, float $totalAmount): void
    {
        $track = $this->payments->find($trackId);
        if (!$track) {
            throw new \RuntimeException('Payment track not found');
        }

        if ($totalAmount < 0) {
            throw new \InvalidArgumentException('Məbləğ mənfi ola bilməz.');
        }

        $track->total_amount = $totalAmount;

        if ($track->paid_amount >= $track->total_amount && $track->total_amount > 0) {
            $track->status = StudentPaymentTrack::STATUS_PAID;
        } elseif ($track->paid_amount > 0) {
            $track->status = 1; // Partial
        } else {
            $track->status = StudentPaymentTrack::STATUS_PENDING;
        }

        $this->payments->save($track);
    }
}
