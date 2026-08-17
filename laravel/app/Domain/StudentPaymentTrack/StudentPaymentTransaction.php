<?php

namespace App\Domain\StudentPaymentTrack;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;

class StudentPaymentTransaction extends Model
{
    protected $fillable = [
        'payment_track_id',
        'amount',
        'paid_at',
        'note',
    ];

    protected function casts(): array
    {
        return [
            'amount' => 'decimal:2',
            'paid_at' => 'datetime',
        ];
    }

    public function track(): BelongsTo
    {
        return $this->belongsTo(StudentPaymentTrack::class, 'payment_track_id');
    }
}
