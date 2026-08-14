<?php

namespace App\Domain\Attendance;

use App\Domain\Attendance\Enums\AttendanceStatus;
use App\Domain\User\User;
use App\Domain\Workspace\Workspace;
use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\SoftDeletes;

class Attendance extends Model
{
    use SoftDeletes;

    public const STATUS_UNKNOWN = AttendanceStatus::UNKNOWN->value;
    public const STATUS_PRESENT = AttendanceStatus::PRESENT->value;
    public const STATUS_ABSENT = AttendanceStatus::ABSENT->value;
    public const STATUS_LATE = AttendanceStatus::LATE->value;
    public const STATUS_EXCUSED = AttendanceStatus::EXCUSED->value;

    public const STATUS_LABELS = [
        self::STATUS_UNKNOWN => 'unknown',
        self::STATUS_PRESENT => 'present',
        self::STATUS_ABSENT => 'absent',
        self::STATUS_LATE => 'late',
        self::STATUS_EXCUSED => 'excused',
    ];

    protected $fillable = [
        'workspace_id',
        'student_id',
        'date',
        'status',
        'note',
    ];

    protected function casts(): array
    {
        return [
            'date' => 'date',
        ];
    }

    public function workspace(): BelongsTo
    {
        return $this->belongsTo(Workspace::class, 'workspace_id');
    }

    public function student(): BelongsTo
    {
        return $this->belongsTo(User::class, 'student_id');
    }

    public function getStatusLabelAttribute(): string
    {
        return AttendanceStatus::labelFor((int) $this->status);
    }
}
