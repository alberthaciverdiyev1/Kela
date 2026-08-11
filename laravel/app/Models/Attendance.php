<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;
use Illuminate\Database\Eloquent\Relations\BelongsTo;
use Illuminate\Database\Eloquent\SoftDeletes;

class Attendance extends Model
{
    use SoftDeletes;

    public const STATUS_UNKNOWN = 0;
    public const STATUS_PRESENT = 1;
    public const STATUS_ABSENT = 2;
    public const STATUS_LATE = 3;
    public const STATUS_EXCUSED = 4;

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
        return self::STATUS_LABELS[$this->status] ?? 'unknown';
    }
}
