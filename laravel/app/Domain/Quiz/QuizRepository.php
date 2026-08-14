<?php

namespace App\Domain\Quiz;

use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;
use Illuminate\Support\Collection;

/**
 * Quiz məlumat girişi üçün kontrakt.
 */
interface QuizRepository
{
    public function create(int $contentId, int $teacherId, array $data): Quiz;

    /** Cədvəl üçün axtarış + səhifələnmiş quiz siyahısı (0 = kök, >0 = qovluq). */
    public function paginateForUser(int $actingUserId, bool $isAdmin, ?string $search, int $folderId = 0, int $perPage = 15): LengthAwarePaginator;

    /** İstifadəçinin görə biləcəyi bütün quizlər (qovluq məhdudiyyəti olmadan — seçim pəncərələri üçün). */
    public function allForUser(int $actingUserId, bool $isAdmin): Collection;

    public function find(int $contentId): ?Quiz;

    /** Sorğunu verilən istifadəçinin görə biləcəyi quizlərlə məhdudlaşdırır. */
    public function scopeForUser(Builder $query, int $actingUserId, bool $isAdmin): Builder;

    public function update(Quiz $quiz, array $data): Quiz;

    /** Quiz-i qovluğa daşıyır (null → kökə). */
    public function moveToFolder(Quiz $quiz, ?int $folderId): Quiz;

    public function delete(Quiz $quiz): bool;

    public function addQuestion(Quiz $quiz, int $questionId, int $position): void;

    public function removeQuestion(Quiz $quiz, int $questionId): void;

    /** Sualın quizdəki mövqeyini (position) yeniləyir. */
    public function setQuestionPosition(Quiz $quiz, int $questionId, int $position): void;

    /** Quiz-dəki sual id-ləri (position sırası ilə). */
    public function questionIds(Quiz $quiz): array;
}
