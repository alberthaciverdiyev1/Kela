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

    public function listForTeacher(int $teacherId, ?string $search = null): Collection;

    /** Sualın istifadə edildiyi quiz sayı. */
    public function usedInQuizzes(Question $question): int;
}
