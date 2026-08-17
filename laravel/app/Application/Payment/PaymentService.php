<?php

namespace App\Application\Payment;

use App\Domain\StudentPaymentTrack\Enums\PaymentStatus;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use App\Domain\StudentPaymentTrack\StudentPaymentTrackRepository;
use App\Domain\Workspace\Workspace;
use App\Domain\Workspace\WorkspaceRepository;
use Illuminate\Support\Collection;
use Illuminate\Support\Facades\DB;

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

    /** Müddəti ötmüş track-ları OVERDUE edir (teacher-ə aid olanlar). */
    public function markOverdueForTeacher(int $teacherId): int
    {
        return $this->payments->markOverdueForTeacher($teacherId);
    }

    /**
     * Tələbə üçün qaimə (track) yaradır, əgər yoxdursa.
     * Teacher-in sahiblik yoxlaması edir; start_date varsa aylıq məbləği proporsional hesablayır.
     * Tələbə hələ sinifə qoşulmayıbsa (gələcək start_date) null qaytarır.
     */
    public function generateInvoice(
        int $teacherId,
        int $studentId,
        int $workspaceId,
        string $month,
        ?float $amount = null,
    ): ?StudentPaymentTrack {
        $this->assertMonth($month);

        /** @var Workspace|null $workspace */
        $workspace = $this->workspaces->find($workspaceId);
        if ($workspace === null) {
            throw new \RuntimeException('Workspace tapılmadı.');
        }
        $this->assertOwnsWorkspace($teacherId, $workspace);

        $existing = $this->payments->findByStudentWorkspaceMonth($studentId, $workspaceId, $month);
        if ($existing) {
            return $existing;
        }

        // Tələbə bu workspace-dədir?
        $studentPivot = $workspace->students()->where('users.id', $studentId)->first();
        if ($studentPivot === null) {
            throw new \RuntimeException('Şagird bu workspace-də deyil.');
        }

        $price = $amount ?? $studentPivot->pivot->agreed_price ?? $workspace->monthly_price ?? 0.0;

        // Hələ qoşulmayıbsa (gələcək start_date) — qaimə yaratma.
        $startDate = $studentPivot->pivot->start_date;
        $price = $this->proratedAmount((float) $price, $startDate, $month);
        if ($price < 0) {
            return null;
        }

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
     * Tələbədən ödəniş almaq. Teacher-in sahiblik yoxlaması + overpayment mühafizəsi.
     */
    public function acceptPayment(int $teacherId, int $trackId, float $amount, ?string $note = null): void
    {
        $track = $this->payments->find($trackId);
        if ($track === null) {
            throw new \RuntimeException('Ödəniş qaiməsi tapılmadı.');
        }
        $this->assertOwnsTrack($teacherId, $track);

        if ($amount <= 0) {
            throw new \InvalidArgumentException('Məbləğ sıfırdan böyük olmalıdır.');
        }

        $debt = (float) $track->total_amount - (float) $track->paid_amount;
        if ($amount > $debt + 0.0001) {
            throw new \InvalidArgumentException('Məbləğ qalıq borcdan çox ola bilməz (qalıq: '.number_format(max($debt, 0), 2).' AZN).');
        }

        DB::transaction(function () use ($track, $teacherId, $amount, $note) {
            $this->payments->createTransaction($track->id, $amount, $note);

            $track->paid_amount = (float) $track->paid_amount + $amount;
            $track->status = $this->statusAfterPayment($track);
            $this->payments->save($track);
        });
    }

    /**
     * Tələbənin ödəniş qaiməsindəki ümumi borc məbləğini yeniləmək.
     * Teacher-in sahiblik yoxlaması + paid_amount-dan aşağı salınma mühafizəsi.
     */
    public function updateTotalAmount(int $teacherId, int $trackId, float $totalAmount): void
    {
        $track = $this->payments->find($trackId);
        if ($track === null) {
            throw new \RuntimeException('Ödəniş qaiməsi tapılmadı.');
        }
        $this->assertOwnsTrack($teacherId, $track);

        if ($totalAmount < 0) {
            throw new \InvalidArgumentException('Məbləğ mənfi ola bilməz.');
        }
        if ($totalAmount < (float) $track->paid_amount) {
            throw new \InvalidArgumentException('Yeni məbləğ ödənilmiş məbləğdən az ola bilməz ('.number_format((float) $track->paid_amount, 2).' AZN ödənilib).');
        }

        $track->total_amount = $totalAmount;
        $track->status = $this->statusAfterPayment($track);
        $this->payments->save($track);
    }

    /** Cari track vəziyyətinə görə status təyin edir. */
    protected function statusAfterPayment(StudentPaymentTrack $track): int
    {
        $total = (float) $track->total_amount;
        $paid = (float) $track->paid_amount;

        if ($total > 0 && $paid >= $total) {
            return StudentPaymentTrack::STATUS_PAID;
        }
        if ($paid > 0) {
            return StudentPaymentTrack::STATUS_PARTIAL;
        }

        return StudentPaymentTrack::STATUS_PENDING;
    }

    /** start_date varsa ay üçün proporsional qiymət; hələ qoşulmayıbsa -1. */
    protected function proratedAmount(float $price, mixed $startDate, string $month): float
    {
        if ($startDate === null || $price <= 0) {
            return $price;
        }

        $start = \Carbon\Carbon::parse($startDate);
        $monthStart = \Carbon\Carbon::createFromFormat('Y-m', $month)->startOfMonth();
        $monthEnd = $monthStart->copy()->endOfMonth();

        // Ay əvvəlində artıq qoşulub → tam qiymət.
        if ($start->lt($monthStart)) {
            return $price;
        }
        // Bu aydan sonra qoşulacaq → hələ qaimə yox.
        if ($start->gt($monthEnd)) {
            return -1.0;
        }
        // Ay içində qoşulub → günə proporsional.
        $daysInMonth = $monthStart->daysInMonth;
        $daysRemaining = $monthEnd->day - $start->day + 1;

        return round($price * $daysRemaining / $daysInMonth, 2);
    }

    protected function assertMonth(string $month): void
    {
        if (! preg_match('/^\d{4}-\d{2}$/', $month)) {
            throw new \InvalidArgumentException('Ay formatı YYYY-MM olmalıdır.');
        }
        $d = \DateTime::createFromFormat('Y-m', $month);
        if ($d === false || $d->format('Y-m') !== $month) {
            throw new \InvalidArgumentException('Ay formatı YYYY-MM olmalıdır.');
        }
    }

    protected function assertOwnsWorkspace(int $teacherId, Workspace $workspace): void
    {
        $user = \App\Domain\User\User::find($teacherId);
        if ($user?->isAdmin()) {
            return;
        }
        if ($workspace->teacher_id !== $teacherId) {
            throw new \RuntimeException('Bu workspace sizə aid deyil.');
        }
    }

    protected function assertOwnsTrack(int $teacherId, StudentPaymentTrack $track): void
    {
        $user = \App\Domain\User\User::find($teacherId);
        if ($user?->isAdmin()) {
            return;
        }
        if ($track->workspace_id !== null && $track->workspace?->teacher_id !== $teacherId) {
            throw new \RuntimeException('Bu ödəniş qaiməsi sizə aid deyil.');
        }
    }
}
