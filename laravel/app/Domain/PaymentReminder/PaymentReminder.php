<?php

namespace App\Domain\PaymentReminder;

use App\Domain\PaymentReminder\Enums\PaymentReminderType;
use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class PaymentReminder extends Model
{
    protected $fillable = [
        'payment_track_id',
        'type',
        'message',
        'sent_at',
    ];

    protected function casts(): array
    {
        return [
            'type' => PaymentReminderType::class,
            'sent_at' => 'datetime',
        ];
    }

    public function track(): BelongsTo
    {
        return $this->belongsTo(StudentPaymentTrack::class, 'payment_track_id');
    }
}
