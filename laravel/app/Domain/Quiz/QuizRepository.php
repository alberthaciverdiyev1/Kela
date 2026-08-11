<?php

namespace App\Domain\Quiz;

/**
 * Quiz məlumat girişi üçün kontrakt.
 */
interface QuizRepository
{
    public function create(int $contentId, int $teacherId, array $data): Quiz;

    public function find(int $contentId): ?Quiz;

    public function update(Quiz $quiz, array $data): Quiz;

    public function delete(Quiz $quiz): bool;

    public function addQuestion(Quiz $quiz, int $questionId, int $position): void;

    public function removeQuestion(Quiz $quiz, int $questionId): void;

    /** Quiz-dəki sual id-ləri (position sırası ilə). */
    public function questionIds(Quiz $quiz): array;
}
