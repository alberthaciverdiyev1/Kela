<?php

namespace App\Application\Question;

use App\Domain\Question\Question;
use App\Domain\Question\QuestionRepository;

/**
 * Sual bankı əməliyyatları. Filament bu servisi çağırır.
 */
class QuestionService
{
    public function __construct(
        private readonly QuestionRepository $questions,
    ) {
    }

    public function find(int $id): ?Question
    {
        return $this->questions->find($id);
    }

    public function create(int $teacherId, array $data): Question
    {
        $this->validate($data);

        return $this->questions->create($teacherId, [
            'text' => trim($data['text']),
            'option_a' => $data['option_a'] ?? null,
            'option_b' => $data['option_b'] ?? null,
            'option_c' => $data['option_c'] ?? null,
            'option_d' => $data['option_d'] ?? null,
            'option_e' => $data['option_e'] ?? null,
            'correct_option' => (int) ($data['correct_option'] ?? 0),
        ]);
    }

    public function update(int $id, array $data, int $actingUserId): Question
    {
        $question = $this->assertOwned($id, $actingUserId);
        $this->validate($data);

        return $this->questions->update($question, [
            'text' => trim($data['text']),
            'option_a' => $data['option_a'] ?? $question->option_a,
            'option_b' => $data['option_b'] ?? $question->option_b,
            'option_c' => $data['option_c'] ?? $question->option_c,
            'option_d' => $data['option_d'] ?? $question->option_d,
            'option_e' => $data['option_e'] ?? $question->option_e,
            'correct_option' => (int) ($data['correct_option'] ?? $question->correct_option),
        ]);
    }

    public function delete(int $id, int $actingUserId): void
    {
        $question = $this->assertOwned($id, $actingUserId);
        $this->questions->delete($question);
    }

    /** Teacher-ın sualları: [id, text, options, correct_option, used_in_quizzes, created_at]. */
    public function listForTeacher(int $teacherId, ?string $search = null): array
    {
        return $this->questions->listForTeacher($teacherId, $search)
            ->map(fn (Question $q) => [
                'id' => $q->id,
                'text' => $q->text,
                'options' => $q->options(),
                'correct_option' => $q->correct_option,
                'used_in_quizzes' => (int) $q->quizzes_count,
                'created_at' => $q->created_at?->toDateTimeString(),
            ])
            ->values()
            ->all();
    }

    public function usedInQuizzes(int $id): int
    {
        $question = $this->questions->find($id);
        if ($question === null) {
            return 0;
        }

        return $this->questions->usedInQuizzes($question);
    }

    protected function assertOwned(int $id, int $actingUserId): Question
    {
        $question = $this->questions->find($id);
        if ($question === null || $question->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Sual tapılmadı.');
        }

        return $question;
    }

    protected function validate(array $data): void
    {
        $text = trim($data['text'] ?? '');
        if ($text === '') {
            throw new \InvalidArgumentException('Sual mətni boş ola bilməz.');
        }

        if (empty($data['option_a']) || empty($data['option_b'])) {
            throw new \InvalidArgumentException('Ən azı A və B seçimləri tələb olunur.');
        }
    }
}
