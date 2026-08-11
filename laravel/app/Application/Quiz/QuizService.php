<?php

namespace App\Application\Quiz;

use App\Domain\Content\Content;
use App\Domain\Content\ContentRepository;
use App\Domain\Question\QuestionRepository;
use App\Domain\Quiz\Quiz;
use App\Domain\Quiz\QuizRepository;

/**
 * Quiz əməliyyatları: CRUD + quiz-sual əlaqəsi.
 * Filament bu servisi çağırır.
 */
class QuizService
{
    public function __construct(
        private readonly QuizRepository $quizzes,
        private readonly ContentRepository $contents,
        private readonly QuestionRepository $questions,
    ) {
    }

    public function find(int $contentId): ?Quiz
    {
        return $this->quizzes->find($contentId);
    }

    /** Quiz + Content (type=quiz) + kitabxana node-u yaradır. */
    public function create(int $teacherId, array $data): Quiz
    {
        $title = trim($data['title'] ?? '');
        if ($title === '') {
            throw new \InvalidArgumentException('Quiz başlığı boş ola bilməz.');
        }

        $content = $this->contents->create([
            'teacher_id' => $teacherId,
            'title' => $title,
            'description' => $data['description'] ?? null,
            'type' => Content::TYPE_QUIZ,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);

        return $this->quizzes->create($content->id, $teacherId, [
            'title' => $title,
            'description' => $data['description'] ?? null,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);
    }

    public function update(int $contentId, array $data): Quiz
    {
        $quiz = $this->quizzes->find($contentId);
        if ($quiz === null) {
            throw new \RuntimeException("Quiz tapılmadı: {$contentId}");
        }

        $quiz = $this->quizzes->update($quiz, [
            'title' => $data['title'] ?? $quiz->title,
            'description' => $data['description'] ?? $quiz->description,
            'is_published' => array_key_exists('is_published', $data)
                ? (bool) $data['is_published']
                : $quiz->is_published,
        ]);

        if ($quiz->content) {
            $this->contents->update($quiz->content, [
                'title' => $quiz->title,
                'description' => $quiz->description,
                'is_published' => $quiz->is_published,
            ]);
        }

        return $quiz;
    }

    /** Quiz + Content (soft) silinir. */
    public function delete(int $contentId): void
    {
        $quiz = $this->quizzes->find($contentId);
        if ($quiz === null) {
            return;
        }

        $this->quizzes->delete($quiz);
        if ($quiz->content) {
            $this->contents->delete($quiz->content);
        }
    }

    /** Quiz-dəki suallar: [position => questionData]. */
    public function questionList(int $contentId): array
    {
        $quiz = $this->quizzes->find($contentId);
        if ($quiz === null) {
            return [];
        }

        return $quiz->questions
            ->map(function ($question) use ($quiz) {
                $position = $quiz->questions->search(fn ($q) => $q->id === $question->id);

                return [
                    'position' => $position !== false ? $position + 1 : 0,
                    'question_id' => $question->id,
                    'text' => $question->text,
                    'options' => $question->options(),
                    'correct_option' => $question->correct_option,
                ];
            })
            ->values()
            ->all();
    }

    /** Quiz-ə sual əlavə edir (position sona). */
    public function addQuestion(int $contentId, int $questionId, int $actingUserId): void
    {
        $quiz = $this->quizzes->find($contentId);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz tapılmadı.');
        }
        if ($quiz->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Bu quizə sual əlavə etmə icazəniz yoxdur.');
        }


        $existing = $this->quizzes->questionIds($quiz);
        if (in_array($questionId, $existing, true)) {
            return; // artıq əlavə olunub
        }

        $this->quizzes->addQuestion($quiz, $questionId, count($existing) + 1);
    }

    public function removeQuestion(int $contentId, int $questionId, int $actingUserId): void
    {
        $quiz = $this->quizzes->find($contentId);
        if ($quiz === null) {
            throw new \RuntimeException('Quiz tapılmadı.');
        }
        if ($quiz->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Bu quizdən sual silmə icazəniz yoxdur.');
        }

        $this->quizzes->removeQuestion($quiz, $questionId);
    }
}
