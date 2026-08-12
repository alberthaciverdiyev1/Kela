<?php

namespace App\Application\Question;

use App\Domain\Question\Question;
use App\Domain\Question\QuestionRepository;

/**
 * Sual bankı əməliyyatları. Web/API bu servisi çağırır.
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

    /** Sual redaktor formu üçün sahə dəyərləri. */
    public function formData(int $id): array
    {
        $question = $this->questions->find($id);
        if ($question === null) {
            return [];
        }

        return [
            'text' => $question->text,
            'option_a' => $question->option_a,
            'option_b' => $question->option_b,
            'option_c' => $question->option_c,
            'option_d' => $question->option_d,
            'option_e' => $question->option_e,
            'correct_option' => (int) $question->correct_option,
        ];
    }

    public function create(int $teacherId, array $data): Question
    {
        $this->validate($data);

        return $this->questions->create($teacherId, [
            'folder_id' => $data['folder_id'] ?? null,
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
            'folder_id' => $data['folder_id'] ?? $question->folder_id,
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

    /** Sualı qovluğa daşıyır (null → kökə). */
    public function moveToFolder(int $id, ?int $folderId, int $actingUserId): Question
    {
        $question = $this->assertOwned($id, $actingUserId);

        return $this->questions->moveToFolder($question, $folderId);
    }

    /** Teacher-ın sualları: [id, text, options, correct_option, used_in_quizzes, created_at]. */
    public function listForTeacher(int $teacherId, ?string $search = null, int $folderId = 0): array
    {
        return $this->questions->listForTeacher($teacherId, $search, $folderId)
            ->map(fn (Question $q) => [
                'id' => $q->id,
                'folder_id' => $q->folder_id,
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
