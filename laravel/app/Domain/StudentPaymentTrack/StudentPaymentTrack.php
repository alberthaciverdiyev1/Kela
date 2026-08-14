<?php

namespace App\Domain\StudentPaymentTrack;

use App\Domain\StudentPaymentTrack\Enums\PaymentStatus;
use App\Domain\User\User;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\SoftDeletes;

class StudentPaymentTrack extends Model
{
    use SoftDeletes;

    public const STATUS_PENDING = PaymentStatus::PENDING->value;
    public const STATUS_PAID = PaymentStatus::PAID->value;
    public const STATUS_OVERDUE = PaymentStatus::OVERDUE->value;
    public const STATUS_CANCELLED = PaymentStatus::CANCELLED->value;

    protected $fillable = [
        'student_id',
        'amount',
        'due_date',
        'status',
    ];

    protected function casts(): array
    {
        return [
            'amount' => 'decimal:2',
            'due_date' => 'datetime',
        ];
    }

    public function student(): BelongsTo
    {
        return $this->belongsTo(User::class, 'student_id');
    }

    public function getStatusLabelAttribute(): string
    {
        return PaymentStatus::labelFor((int) $this->status);
    }
}
