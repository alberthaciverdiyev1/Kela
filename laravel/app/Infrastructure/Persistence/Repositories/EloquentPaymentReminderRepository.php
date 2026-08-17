<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\PaymentReminder\Enums\PaymentReminderType;
use App\Domain\PaymentReminder\PaymentReminder;
use App\Domain\PaymentReminder\PaymentReminderRepository;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;

class EloquentPaymentReminderRepository implements PaymentReminderRepository
{
    public function createIfMissing(StudentPaymentTrack $track, PaymentReminderType $type, string $message): bool
    {
        $exists = PaymentReminder::where('payment_track_id', $track->id)
            ->where('type', $type->value)
            ->exists();

        if ($exists) {
            return false;
        }

        PaymentReminder::create([
            'payment_track_id' => $track->id,
            'type' => $type,
            'message' => $message,
            'sent_at' => now(),
        ]);

        return true;
    }

    public function forTeacher(int $teacherId, int $perPage = 50): LengthAwarePaginator
    {
        return PaymentReminder::query()
            ->with([
                'track.student' => fn ($q) => $q->withTrashed(),
                'track.workspace',
            ])
            ->whereHas('track', function ($q) use ($teacherId) {
                $q->whereHas('workspace', fn ($w) => $w->where('teacher_id', $teacherId));
            })
            ->orderByDesc('sent_at')
            ->paginate($perPage);
    }
}
