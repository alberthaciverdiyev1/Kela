<?php

namespace App\Application\Homework;

use App\Domain\Homework\Enums\HomeworkQuestionType;
use App\Domain\Homework\Homework;
use App\Domain\Homework\HomeworkQuestion;
use App\Domain\Homework\HomeworkRepository;
use App\Domain\Quiz\QuizRepository;
use App\Domain\User\User;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;

/**
 * Ev tapşırığı əməliyyatları: CRUD + sual kompozisiyası.
 *
 * Sual mənbələri:
 *   - QUIZ tipi — müəllimin öz quizlərindən seçilən, variantlı sual (anlıq görüntü).
 *   - TASK tipi — əl ilə yazılmış, variantsız tapşırıq sualı.
 */
class HomeworkService
{
    public function __construct(
        private readonly HomeworkRepository $homeworks,
        private readonly QuizRepository $quizzes,
    ) {
    }

    public function find(int $id): ?Homework
    {
        return $this->homeworks->find($id);
    }

    /** Cədvəl üçün axtarış + səhifələnmiş ev tapşırığı siyahısı (array DTO). */
    public function paginate(int $actingUserId, ?string $search = null, int $perPage = 15): LengthAwarePaginator
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->homeworks
            ->paginateForUser($actingUserId, $isAdmin, $search, $perPage)
            ->through(fn (Homework $homework): array => [
                'id' => (int) $homework->id,
                'title' => $homework->title,
                'description' => $homework->description,
                'questions_count' => (int) ($homework->questions_count ?? 0),
                'is_published' => (bool) $homework->is_published,
                'created_at' => $homework->created_at?->format('d M Y'),
            ]);
    }

    /** Editör/forma üçün başlıq/təsvir/yayım statusu. */
    public function formData(int $id): array
    {
        $homework = $this->homeworks->find($id);
        if ($homework === null) {
            return [];
        }

        return [
            'id' => (int) $homework->id,
            'title' => $homework->title,
            'description' => $homework->description,
            'is_published' => (bool) $homework->is_published,
        ];
    }

    /** Ev tapşırığı yaradır. */
    public function create(int $teacherId, array $data): Homework
    {
        $title = trim((string) ($data['title'] ?? ''));
        if ($title === '') {
            throw new \InvalidArgumentException('Ev tapşırığının başlığı boş ola bilməz.');
        }

        $homework = $this->homeworks->create($teacherId, [
            'title' => $title,
            'description' => $data['description'] ?? null,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);

        $this->saveQuestions($homework, $data['questions'] ?? []);

        return $homework;
    }

    /** Ev tapşırığını yeniləyir (suallar tam dəyişdirilir). */
    public function update(int $id, array $data, int $actingUserId): Homework
    {
        $homework = $this->assertOwned($id, $actingUserId);

        $this->homeworks->update($homework, [
            'title' => trim((string) $data['title']) ?: $homework->title,
            'description' => $data['description'] ?? null,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);

        $this->saveQuestions($homework, $data['questions'] ?? []);

        return $homework;
    }

    public function delete(int $id, int $actingUserId): void
    {
        $homework = $this->assertOwned($id, $actingUserId);
        $this->homeworks->delete($homework);
    }

    /** Ev tapşırığının sualları (editor üçün DTO massivi). */
    public function questionList(int $id): array
    {
        $homework = $this->homeworks->find($id);
        if ($homework === null) {
            return [];
        }

        return array_map(fn (HomeworkQuestion $q) => [
            'type' => (int) $q->type,
            'text' => $q->text,
            'options' => $q->options(),
            'correct_option' => $q->correct_option !== null ? (int) $q->correct_option : null,
            'source_question_id' => $q->source_question_id ? (int) $q->source_question_id : null,
            'source_quiz_id' => $q->source_quiz_id ? (int) $q->source_quiz_id : null,
        ], $this->homeworks->questions($homework));
    }

    /** "Quizdən əlavə et" pəncərəsi üçün müəllimin quiz siyahısı: [id, title]. */
    public function quizOptions(int $actingUserId): array
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        $options = [];
        foreach ($this->quizzes->paginateForUser($actingUserId, $isAdmin, null, 0, 500) as $quiz) {
            $options[] = [
                'id' => (int) $quiz->content_id,
                'title' => $quiz->title,
            ];
        }

        return $options;
    }

    /**
     * Verilmiş quizin suallarını "Quizdən əlavə et" pəncərəsi üçün qaytarır.
     * Sual mətni + variantlar anlıq görüntü kimi saxlanacaq.
     */
    public function quizQuestions(int $quizId, int $actingUserId): array
    {
        $quiz = $this->quizzes->find($quizId);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz tapılmadı.');
        }
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;
        if (! $isAdmin && $quiz->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Bu quizə baxmağa icazəniz yoxdur.');
        }

        $result = [];
        foreach ($quiz->questions as $question) {
            $position = $quiz->questions->search(fn ($q) => $q->id === $question->id);
            $result[] = [
                'question_id' => (int) $question->id,
                'text' => $question->text,
                'options' => $question->options(),
                'correct_option' => $question->correct_option !== null ? (int) $question->correct_option : null,
                'position' => $position !== false ? $position + 1 : 0,
            ];
        }

        return $result;
    }

    /**
     * Sualları yoxlayıb ev tapşırığına yazır.
     * Hər sual: { type, text, option_a..e, correct_option, source_question_id, source_quiz_id }
     */
    protected function saveQuestions(Homework $homework, array $questions): void
    {
        $normalized = [];

        foreach ($questions as $item) {
            $item = (array) $item;
            $type = (int) ($item['type'] ?? HomeworkQuestionType::TASK->value);
            if (! HomeworkQuestionType::isValid($type)) {
                $type = HomeworkQuestionType::TASK->value;
            }

            $text = trim(strip_tags((string) ($item['text'] ?? '')));
            if ($text === '') {
                throw new \InvalidArgumentException('Sual mətni boş ola bilməz.');
            }

            $normalized[] = [
                'type' => $type,
                'text' => $text,
                'option_a' => $this->nullable($item['option_a'] ?? null),
                'option_b' => $this->nullable($item['option_b'] ?? null),
                'option_c' => $this->nullable($item['option_c'] ?? null),
                'option_d' => $this->nullable($item['option_d'] ?? null),
                'option_e' => $this->nullable($item['option_e'] ?? null),
                'correct_option' => isset($item['correct_option']) && $item['correct_option'] !== ''
                    ? (int) $item['correct_option']
                    : null,
                'source_question_id' => $this->nullableInt($item['source_question_id'] ?? null),
                'source_quiz_id' => $this->nullableInt($item['source_quiz_id'] ?? null),
            ];
        }

        $this->homeworks->replaceQuestions($homework, $normalized);
    }

    protected function assertOwned(int $id, int $actingUserId): Homework
    {
        $homework = $this->homeworks->find($id);
        if ($homework === null) {
            throw new \RuntimeException('Ev tapşırığı tapılmadı.');
        }
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;
        if (! $isAdmin && $homework->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Bu ev tapşırığına icazəniz yoxdur.');
        }

        return $homework;
    }

    protected function nullable(mixed $value): ?string
    {
        $value = trim((string) ($value ?? ''));
        return $value === '' ? null : $value;
    }

    protected function nullableInt(mixed $value): ?int
    {
        $value = (string) ($value ?? '');
        return $value === '' ? null : (int) $value;
    }
}
