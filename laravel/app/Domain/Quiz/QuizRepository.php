<?php

namespace App\Domain\Quiz;

use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;

/**
 * Quiz məlumat girişi üçün kontrakt.
 */
interface QuizRepository
{
    public function create(int $contentId, int $teacherId, array $data): Quiz;

    /** Cədvəl üçün axtarış + səhifələnmiş quiz siyahısı. */
    public function paginateForUser(int $actingUserId, bool $isAdmin, ?string $search, int $perPage): LengthAwarePaginator;

    public function find(int $contentId): ?Quiz;

    /** Sorğunu verilən istifadəçinin görə biləcəyi quizlərlə məhdudlaşdırır. */
    public function scopeForUser(Builder $query, int $actingUserId, bool $isAdmin): Builder;

    public function update(Quiz $quiz, array $data): Quiz;

    public function delete(Quiz $quiz): bool;

    public function addQuestion(Quiz $quiz, int $questionId, int $position): void;

    public function removeQuestion(Quiz $quiz, int $questionId): void;

    /** Sualın quizdəki mövqeyini (position) yeniləyir. */
    public function setQuestionPosition(Quiz $quiz, int $questionId, int $position): void;

    /** Quiz-dəki sual id-ləri (position sırası ilə). */
    public function questionIds(Quiz $quiz): array;
}
