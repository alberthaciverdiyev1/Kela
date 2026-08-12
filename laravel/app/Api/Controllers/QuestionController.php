<?php

namespace App\Api\Controllers;

use App\Application\Question\QuestionService;
use App\Application\QuestionFolder\QuestionFolderService;
use App\Domain\Question\Question;
use App\Api\Resources\QuestionResource;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class QuestionController
{
    public function __construct(
        private readonly QuestionService $questions,
        private readonly QuestionFolderService $folders,
    ) {
    }

    /** QuestionService::listForTeacher array DTO döndürür → birbaşa JSON. */
    public function index(Request $request): JsonResponse
    {
        return response()->json([
            'data' => $this->questions->listForTeacher(
                (int) $request->user()->id,
                $request->string('search')->toString() ?: null,
                (int) $request->integer('folder_id'),
            ),
        ]);
    }

    public function store(Request $request): JsonResponse
    {
        $data = $request->validate([
            'text' => ['required', 'string'],
            'option_a' => ['required', 'string'],
            'option_b' => ['required', 'string'],
            'option_c' => ['nullable', 'string'],
            'option_d' => ['nullable', 'string'],
            'option_e' => ['nullable', 'string'],
            'correct_option' => ['required', 'integer', 'min:0', 'max:4'],
            'folder_id' => ['nullable', 'integer'],
        ]);

        $data['folder_id'] = $this->folders->resolveFolderFor(
            (int) $request->user()->id,
            $data['folder_id'] ?? null,
        );

        $question = $this->questions->create((int) $request->user()->id, $data);

        return (new QuestionResource($question))->response()->setStatusCode(201);
    }

    public function show(int $question): QuestionResource
    {
        $model = $this->questions->find($question);
        $this->authorizeAccess($model);

        return new QuestionResource($model);
    }

    public function update(Request $request, int $question): QuestionResource
    {
        $data = $request->validate([
            'text' => ['required', 'string'],
            'option_a' => ['required', 'string'],
            'option_b' => ['required', 'string'],
            'option_c' => ['nullable', 'string'],
            'option_d' => ['nullable', 'string'],
            'option_e' => ['nullable', 'string'],
            'correct_option' => ['required', 'integer', 'min:0', 'max:4'],
            'folder_id' => ['nullable', 'integer'],
        ]);

        $data['folder_id'] = $this->folders->resolveFolderFor(
            (int) $request->user()->id,
            $data['folder_id'] ?? null,
        );

        return new QuestionResource(
            $this->questions->update($question, $data, (int) $request->user()->id),
        );
    }

    public function destroy(Request $request, int $question): JsonResponse
    {
        $this->questions->delete($question, (int) $request->user()->id);

        return response()->json(['message' => 'Sual silindi.']);
    }

    private function authorizeAccess(?Question $question): void
    {
        if ($question === null) {
            abort(404);
        }
        $user = request()->user();
        if ($user->isAdmin()) {
            return;
        }
        if ($question->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
