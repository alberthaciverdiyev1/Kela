<?php

namespace App\Domain\Question;

use Illuminate\Support\Collection;

/**
 * Sual bankı (Question) məlumat girişi üçün kontrakt.
 */
interface QuestionRepository
{
    public function create(int $teacherId, array $data): Question;

    public function update(Question $question, array $data): Question;

    public function delete(Question $question): bool;

    public function find(int $id): ?Question;

    /** Teacher-ın sualları. folderId null → yalnız kök (qovluqsuz), 0 → hamısı. */
    public function listForTeacher(int $teacherId, ?string $search = null, int $folderId = 0): Collection;

    /** Sualı qovluğa daşıyır (null → kökə). */
    public function moveToFolder(Question $question, ?int $folderId): Question;

    /** Sualın istifadə edildiyi quiz sayı. */
    public function usedInQuizzes(Question $question): int;
}
