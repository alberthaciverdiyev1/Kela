<?php

namespace App\Domain\Lesson;

use Illuminate\Database\Eloquent\Builder;

/**
 * Dərs (Lesson) məlumat girişi üçün kontrakt.
 * Implementasiya Infrastructure katmanında (Eloquent) yerləşir.
 */
interface LessonRepository
{
    public function create(array $attributes): Lesson;

    public function update(Lesson $lesson, array $attributes): Lesson;

    public function delete(Lesson $lesson): bool;

    public function find(int $contentId): ?Lesson;

    /** Sorğunu verilən istifadəçinin görə biləcəyi dərslərlə məhdudlaşdırır. */
    public function scopeForUser(Builder $query, int $actingUserId, bool $isAdmin): Builder;
}
