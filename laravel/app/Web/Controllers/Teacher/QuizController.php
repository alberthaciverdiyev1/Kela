<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Question\QuestionService;
use App\Application\Quiz\QuizService;
use App\Domain\Quiz\Quiz;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Quiz səhifələri — server-rendered Blade.
 * Sual əməliyyatları JS vasitəsilə /api/v1-ə gedir; buradakı fragment-lər
 * server-rendered olub yalnız lazım olan bölməni yeniləyir.
 */
class QuizController
{
    public function __construct(
        private readonly QuizService $quizzes,
        private readonly QuestionService $questions,
    ) {
    }

    public function index(Request $request): View
    {
        $search = (string) $request->string('search');

        return view('teacher.quizzes.index', [
            'quizzes' => $this->quizzes->paginate((int) auth()->id(), $search ?: null, 15),
            'search' => $search,
        ]);
    }

    public function create(): View
    {
        return view('teacher.quizzes.form', [
            'heading' => 'Yeni Quiz',
            'subtitle' => 'Yeni quiz əlavə et',
            'creating' => true,
            'quiz' => null,
        ]);
    }

    public function store(Request $request): RedirectResponse
    {
        $data = $this->validated($request);

        $quiz = $this->quizzes->create((int) auth()->id(), $data);

        return redirect()
            ->route('teacher.quizzes.edit', $quiz->getKey())
            ->with('success', 'Quiz yaradıldı — sual əlavə etmək üçün redaktoru açın.');
    }

    public function edit(int $quiz): View
    {
        $this->assertAccess($this->quizzes->find($quiz));

        return view('teacher.quizzes.edit', [
            'contentId' => $quiz,
            'quiz' => $this->quizzes->formData($quiz),
            'questions' => $this->quizzes->questionList($quiz),
            'bankOptions' => $this->bankOptions($quiz),
        ]);
    }

    /** JS-in sual siyahısını yeniləməsi üçün server-rendered fragment. */
    public function questionsFragment(int $quiz): View
    {
        $this->assertAccess($this->quizzes->find($quiz));

        return view('teacher.quizzes._questions', [
            'contentId' => $quiz,
            'questions' => $this->quizzes->questionList($quiz),
        ]);
    }

    public function update(Request $request, int $quiz): RedirectResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $data = $this->validated($request);

        $this->quizzes->update($quiz, $data);

        return redirect()->route('teacher.quizzes.edit', $quiz)->with('success', 'Quiz yeniləndi.');
    }

    public function destroy(int $quiz): RedirectResponse
    {
        $this->assertAccess($this->quizzes->find($quiz));

        $this->quizzes->delete($quiz);

        return redirect()->route('teacher.quizzes.index')->with('success', 'Quiz silindi.');
    }

    protected function validated(Request $request): array
    {
        $data = $request->validate([
            'title' => ['required', 'string', 'max:200'],
            'description' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
        ]);

        $data['is_published'] = $request->boolean('is_published');

        return $data;
    }

    /** Sual bankındakı mövcud suallar (quizə əlavə olunmayanlar): [id => text]. */
    protected function bankOptions(int $contentId): array
    {
        $options = [];

        foreach ($this->quizzes->availableQuestionIds($contentId, (int) auth()->id()) as $id) {
            $data = $this->questions->formData($id);
            if ($data !== []) {
                $options[$id] = $data['text'];
            }
        }

        return $options;
    }

    protected function assertAccess(?Quiz $quiz): void
    {
        if ($quiz === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $quiz->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
