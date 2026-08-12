<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Question\QuestionService;
use App\Application\QuestionFolder\QuestionFolderService;
use App\Domain\QuestionFolder\QuestionFolder;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Sual bankı səhifələri — server-rendered Blade.
 * Kataloq (qovluq + sual) GET ilə server tərəfindən render olunur;
 * qovluq/sual əməliyyatları JS → /api/v1 vasitəsilə aparılır.
 */
class QuestionController
{
    public function __construct(
        private readonly QuestionService $questions,
        private readonly QuestionFolderService $folders,
    ) {
    }

    public function index(Request $request): View
    {
        $folderId = $request->integer('folder_id') ?: null;
        $this->assertAccess($folderId);

        $dir = $this->folders->directory((int) auth()->id(), $folderId);

        return view('teacher.questions.index', [
            'folderId' => $folderId,
            'folders' => $dir['folders'],
            'questions' => $dir['questions'],
            'breadcrumbs' => $dir['breadcrumbs'],
            'folderTree' => $this->folders->folderTree((int) auth()->id()),
            'fragmentUrl' => route('teacher.questions.table', ['folder_id' => $folderId ?? null]),
        ]);
    }

    /** JS-in kataloqu yeniləməsi üçün server-rendered fragment. */
    public function tableFragment(Request $request): View
    {
        $folderId = $request->integer('folder_id') ?: null;
        $this->assertAccess($folderId);

        $dir = $this->folders->directory((int) auth()->id(), $folderId);

        return view('teacher.questions._table', [
            'folderId' => $folderId,
            'folders' => $dir['folders'],
            'questions' => $dir['questions'],
            'breadcrumbs' => $dir['breadcrumbs'],
            'folderTree' => $this->folders->folderTree((int) auth()->id()),
        ]);
    }

    /** Qovluq verilərsə sahibliyi yoxla (admin hər zaman, başqası 403). */
    protected function assertAccess(?int $folderId): void
    {
        if ($folderId === null) {
            return;
        }
        $folder = $this->folders->find($folderId);
        if ($folder === null) {
            abort(404);
        }
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $folder->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
