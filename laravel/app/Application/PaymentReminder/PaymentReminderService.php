<?php

namespace App\Application\PaymentReminder;

use App\Domain\PaymentReminder\Enums\PaymentReminderType;
use App\Domain\PaymentReminder\PaymentReminderRepository;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use App\Domain\StudentPaymentTrack\StudentPaymentTrackRepository;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;

/**
 * Ödəniş bildirişləri: saatlıq cron `payments:remind` tərəfindən çağrılır.
 *
 * Qaydalar:
 *  - due_date-indən 5 gün əvvəl "upcoming" (yaxınlaşan) bildirişi.
 *  - due_date günü (və ya ilk müşahidədə keçmişdirsə) "due" bildirişi.
 *  - Hər track+tip üçün yalnız 1 dəfə — təkrar göndərilmir.
 *  - Yalnız ödənilməmiş/qismən ödənilmiş track-lar üçün.
 */
class PaymentReminderService
{
    public function __construct(
        private readonly StudentPaymentTrackRepository $payments,
        private readonly PaymentReminderRepository $reminders,
    ) {
    }

    /** Saatlıq cron. Yeni yaradılan bildiriş sayını qaytarır. */
    public function run(): int
    {
        $now = now();
        $cutoff = $now->copy()->addDays(5)->endOfDay();
        $sent = 0;

        foreach ($this->payments->unpaidDueBy($cutoff) as $track) {
            $type = $this->determineType($track, $now);
            if ($type === null) {
                continue;
            }

            if ($this->reminders->createIfMissing($track, $type, $this->buildMessage($track, $type))) {
                $sent++;
            }
        }

        return $sent;
    }

    /** Teacher-ə aid bütün bildirişlər (səhifələnmiş, ən yenisindən köhnəyə). */
    public function listForTeacher(int $teacherId): LengthAwarePaginator
    {
        return $this->reminders->forTeacher($teacherId);
    }

    protected function determineType(StudentPaymentTrack $track, \Carbon\CarbonInterface $now): ?PaymentReminderType
    {
        $due = $track->due_date;
        if ($due === null) {
            return null;
        }

        if ($due->isToday() || $due->lt($now)) {
            return PaymentReminderType::DUE;
        }
        if ($due->lte($now->copy()->addDays(5)->endOfDay())) {
            return PaymentReminderType::UPCOMING;
        }

        return null;
    }

    protected function buildMessage(StudentPaymentTrack $track, PaymentReminderType $type): string
    {
        $studentName = $track->student?->full_name ?? ('Şagird #'.$track->student_id);
        $debt = max(0, (float) $track->total_amount - (float) $track->paid_amount);
        $amount = number_format($debt, 2);
        $due = $track->due_date->format('d.m.Y');

        if ($type === PaymentReminderType::DUE) {
            $verb = $track->due_date->isToday() ? 'bugün bitir' : 'müddəti keçib';
            return "{$studentName} şagirdinin ödəniş {$verb}! Qalıq borc: {$amount} AZN (son tarix: {$due}).";
        }

        $daysLeft = max(1, (int) ceil($track->due_date->copy()->startOfDay()->diffInDays(now()->copy()->startOfDay())));
        return "{$studentName} şagirdinin ödəniş müddətinə {$daysLeft} gün qalıb. Qalıq borc: {$amount} AZN (son tarix: {$due}).";
    }
}
