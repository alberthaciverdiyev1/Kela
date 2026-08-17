<?php

namespace App\Domain\PaymentReminder;

use App\Domain\PaymentReminder\Enums\PaymentReminderType;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;

interface PaymentReminderRepository
{
    /**
     * Track üçün tipə görə bildiriş mövcud deyilsə yaradır (unikal `payment_track_id+type`).
     *
     * @return bool yeni bildiriş yaradıldısa true
     */
    public function createIfMissing(StudentPaymentTrack $track, PaymentReminderType $type, string $message): bool;

    /**
     * Teacher-ə aid (track → workspace.teacher_id) bütün bildirişlər,
     * ən yenisindən köhnəyə səhifələnmiş.
     */
    public function forTeacher(int $teacherId, int $perPage = 50): LengthAwarePaginator;
}
