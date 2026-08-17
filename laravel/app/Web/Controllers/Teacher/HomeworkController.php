<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Homework\HomeworkService;
use App\Domain\Homework\Homework;
use App\Http\Requests\HomeworkRequest;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Ev tapşırığı səhifələri — server-rendered Blade.
 *
 * Sual kompozisiyası editor səhifəsində (Alpine) aparılır; suallar yadda
 * saxlanarkən gizli questions_json sahəsi ilə servisə ötürülür.
 * "Quizdən əlavə et" pəncərəsi sual mənbəyini mövcud API-dən alır.
 */
class HomeworkController
{
    public function __construct(
        private readonly HomeworkService $homeworks,
    )
    {
    }

    public function index(Request $request): View
    {
        $search = (string)$request->string('search');

        return view('teacher.homeworks.index', [
            'homeworks' => $this->homeworks->paginate((int)auth()->id(), $search ?: null, 15),
            'search' => $search,
        ]);
    }

    public function create(): View
    {
        return view('teacher.homeworks.form', [
            'heading' => 'Yeni Ev Tapşırığı',
            'subtitle' => 'Sualları quizlərdən seçin və ya əl ilə tapşırıq yazın',
            'creating' => true,
            'homework' => null,
            'questions' => [],
        ]);
    }

    public function store(HomeworkRequest $request): RedirectResponse
    {
        $data = $this->validated($request);

        $homework = $this->homeworks->create((int)auth()->id(), $data);

        return redirect()
            ->route('teacher.homeworks.show', $homework->id)
            ->with('success', 'Ev tapşırığı yaradıldı.');
    }

    /** Ev tapşırığının ətraflı görünüşü (sual siyahısı ilə). */
    public function show(int $homework): View
    {
        $model = $this->homeworks->find($homework);
        $this->assertAccess($model);

        return view('teacher.homeworks.show', [
            'homework' => $model,
            'questions' => $this->homeworks->questionList($homework),
        ]);
    }

    public function edit(int $homework): View
    {
        $model = $this->homeworks->find($homework);
        $this->assertAccess($model);

        return view('teacher.homeworks.form', [
            'heading' => 'Ev Tapşırığını Düzləndir',
            'subtitle' => 'Başlığı və sualları yeniləyin',
            'creating' => false,
            'homework' => $this->homeworks->formData($homework),
            'questions' => $this->homeworks->questionList($homework),
        ]);
    }

    public function update(HomeworkRequest $request, int $homework): RedirectResponse
    {
        $this->assertAccess($this->homeworks->find($homework));

        $data = $this->validated($request);
        $this->homeworks->update($homework, $data, (int)auth()->id());

        return redirect()->route('teacher.homeworks.show', $homework)->with('success', 'Ev tapşırığı yeniləndi.');
    }

    public function destroy(int $homework): RedirectResponse
    {
        $this->assertAccess($this->homeworks->find($homework));

        $this->homeworks->delete($homework, (int)auth()->id());

        return redirect()->route('teacher.homeworks.index')->with('success', 'Ev tapşırığı silindi.');
    }

    /** "Quizdən əlavə et" pəncərəsi üçün seçilmiş quizin sualları (JSON). */
    public function quizQuestions(Request $request, int $quizId): \Illuminate\Http\JsonResponse
    {
        try {
            $questions = $this->homeworks->quizQuestions($quizId, (int)auth()->id());
        } catch (\RuntimeException $e) {
            abort(403);
        }

        return response()->json(['questions' => $questions]);
    }

    protected function validated(HomeworkRequest $request): array
    {
        $data = $request->validated();

        $data['is_published'] = $request->boolean('is_published');

        $data['questions'] = [];
        if (!empty($data['questions_json'])) {
            $decoded = json_decode($data['questions_json'], true);
            if (is_array($decoded)) {
                $data['questions'] = $decoded;
            }
        }

        return $data;
    }

    protected function assertAccess(?Homework $homework): void
    {
        if ($homework === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $homework->teacher_id !== (int)$user->id) {
            abort(403);
        }
    }
}
