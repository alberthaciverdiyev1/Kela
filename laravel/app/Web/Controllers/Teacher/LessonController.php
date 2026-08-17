<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Lesson\LessonService;
use App\Application\LessonFolder\LessonFolderService;
use App\Application\WorkspaceFolder\WorkspaceFolderService;
use App\Domain\Lesson\Lesson;
use App\Domain\LessonFolder\LessonFolder;
use App\Http\Requests\LessonRequest;
use App\Http\Requests\MoveFolderRequest;
use App\Http\Requests\MoveLessonRequest;
use App\Http\Requests\RenameFolderRequest;
use App\Http\Requests\StoreFolderRequest;
use App\Infrastructure\Media\MediaProcessor;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Dərs səhifələri — server-rendered Blade.
 * Bütün əməliyyatlar LessonService üzərindən; modellərə birbaşa toxunulmur.
 */
class LessonController
{
    public function __construct(
        private readonly LessonService $lessons,
        private readonly LessonFolderService $lessonFolders,
        private readonly WorkspaceFolderService $workspaceFolders,
    ) {
    }

    public function index(Request $request): View
    {
        $search = (string) $request->string('search');
        $folderId = (int) $request->integer('folder_id');

        return view('teacher.lessons.index', [
            'lessons' => $this->lessons->paginate((int) auth()->id(), $search ?: null, $folderId, 15),
            'search' => $search,
            'folderId' => $folderId,
            'folders' => $this->lessonFolders->directory((int) auth()->id(), $folderId ?: null),
            'folderTree' => $this->lessonFolders->folderTree((int) auth()->id()),
        ]);
    }

    public function create(Request $request): View
    {
        $selectedFolder = $this->lessonFolders->resolveFolderFor((int) auth()->id(), (int) $request->integer('folder_id') ?: null);

        return view('teacher.lessons.form', [
            'heading' => 'Yeni Dərs',
            'subtitle' => 'Yeni dərs əlavə et',
            'creating' => true,
            'lesson' => null,
            'folderTree' => $this->lessonFolders->folderTree((int) auth()->id()),
            'selectedFolderId' => $selectedFolder,
            'workspaceContext' => $this->workspaceFolders->resolveContextFor(
                (int) $request->integer('workspace_id') ?: null,
                (int) $request->integer('ws_folder_id') ?: null,
                (int) auth()->id(),
            ),
        ]);
    }

    public function store(LessonRequest $request): RedirectResponse
    {
        $data = $request->validated();
        $data['is_published'] = $request->boolean('is_published');
        $data['order_index'] = (int) ($data['order_index'] ?? 0);
        $data['folder_id'] = $this->lessonFolders->resolveFolderFor((int) auth()->id(), $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) $request->integer('workspace_id') ?: null,
            (int) $request->integer('ws_folder_id') ?: null,
            (int) auth()->id(),
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        $path = $this->storeVideo($request);
        if ($path !== null) {
            $data['video_path'] = $path;
        }

        $this->lessons->create((int) auth()->id(), $data);

        return redirect()->route('teacher.lessons.index')->with('success', 'Dərs yaradıldı.');
    }

    public function show(int $lesson): View
    {
        $model = $this->lessons->find($lesson);
        if ($model === null) {
            abort(404);
        }
        $this->assertAccess($model);

        $viewer = $this->lessons->viewerData($lesson);

        return view('teacher.lessons.view', [
            'contentId' => $lesson,
            'lessonData' => [
                'title' => $model->content?->title ?? '',
                'description' => $model->content?->description,
                'is_published' => (bool) $model->is_published,
                'duration_label' => $model->duration_label,
                'order_index' => (int) $model->order_index,
                'created_at' => fmt_date($model->created_at, 'd M Y H:i'),
            ],
            'hasVideo' => (bool) ($viewer['hasVideo'] ?? false),
            'streamUrl' => $viewer['streamUrl'] ?? '',
            'thumbUrl' => $viewer['thumbUrl'] ?? null,
        ]);
    }

    public function edit(int $lesson): View
    {
        $model = $this->lessons->find($lesson);
        if ($model === null) {
            abort(404);
        }
        $this->assertAccess($model);

        return view('teacher.lessons.form', [
            'heading' => 'Dərsi Redaktə Et',
            'subtitle' => $model->content?->title ?? '',
            'creating' => false,
            'lesson' => [
                'content_id' => $lesson,
                'title' => $model->content?->title ?? '',
                'description' => $model->content?->description ?? '',
                'video_path' => $model->video_path ?? '',
                'folder_id' => $model->folder_id ? (int) $model->folder_id : null,
                'is_published' => (bool) $model->is_published,
                'order_index' => (int) $model->order_index,
            ],
            'folderTree' => $this->lessonFolders->folderTree((int) auth()->id()),
            'workspaceContext' => $this->workspaceFolders->resolveContextFor(
                (int) ($model->content?->workspace_id ?? 0) ?: null,
                (int) ($model->content?->folder_id ?? 0) ?: null,
                (int) auth()->id(),
            ),
        ]);
    }

    public function update(LessonRequest $request, int $lesson): RedirectResponse
    {
        $model = $this->lessons->find($lesson);
        if ($model === null) {
            abort(404);
        }
        $this->assertAccess($model);

        $data = $request->validated();
        $data['is_published'] = $request->boolean('is_published');
        $data['order_index'] = (int) ($data['order_index'] ?? 0);
        $data['folder_id'] = $this->lessonFolders->resolveFolderFor((int) auth()->id(), $data['folder_id'] ?? null);

        $context = $this->workspaceFolders->resolveContextFor(
            (int) $request->integer('workspace_id') ?: null,
            (int) $request->integer('ws_folder_id') ?: null,
            (int) auth()->id(),
        );
        if ($context !== null) {
            $data['workspace_id'] = $context['workspace_id'];
            $data['ws_folder_id'] = $context['folder_id'];
        }

        $newPath = $this->storeVideo($request);
        $data['video_path'] = $newPath ?? $model->video_path;

        $this->lessons->update($lesson, $data);

        return redirect()->route('teacher.lessons.index')->with('success', 'Dərs yeniləndi.');
    }

    public function destroy(Request $request, int $lesson): RedirectResponse|JsonResponse
    {
        $model = $this->lessons->find($lesson);
        if ($model === null) {
            abort(404);
        }
        $this->assertAccess($model);

        $this->lessons->delete($lesson);

        if ($request->expectsJson()) {
            return response()->json(['message' => 'Dərs silindi.']);
        }

        return redirect()->route('teacher.lessons.index')->with('success', 'Dərs silindi.');
    }

    // ─────────────────────────────────────────────────────────────────────────
    // JSON əməliyyatları — frontend JS bu endpointləri çağırır (web controller).
    // ─────────────────────────────────────────────────────────────────────────

    /** Yeni dərs qovluğu yaradır. */
    public function storeFolder(StoreFolderRequest $request): JsonResponse
    {
        $data = $request->validated();

        $folder = $this->lessonFolders->createFolder(
            (int) auth()->id(),
            $data['name'],
            $data['parent_id'] ?? null,
        );

        return response()->json([
            'data' => ['id' => (int) $folder->id, 'name' => $folder->name],
        ], 201);
    }

    /** Dərs qovluğunun adını dəyişir. */
    public function renameFolder(RenameFolderRequest $request, int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $data = $request->validated();

        $this->lessonFolders->renameFolder((int) auth()->id(), $folderId, $data['name']);

        return response()->json(['message' => 'Qovluq adı yeniləndi.']);
    }

    /** Dərs qovluğunu daşıyır (null → kök). */
    public function moveFolder(MoveFolderRequest $request, int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $data = $request->validated();

        $this->lessonFolders->moveFolder((int) auth()->id(), $folderId, $data['parent_id'] ?? null);

        return response()->json(['message' => 'Qovluq daşındı.']);
    }

    /** Dərs qovluğunu silir (dərslər kökə qayıdır). */
    public function destroyFolder(int $folderId): JsonResponse
    {
        $this->assertFolderAccess($folderId);

        $this->lessonFolders->deleteFolder((int) auth()->id(), $folderId);

        return response()->json(['message' => 'Qovluq silindi.']);
    }

    /** Dərsi qovluğa daşıyır (null → kök). */
    public function moveLessonToFolder(MoveLessonRequest $request): JsonResponse
    {
        $data = $request->validated();

        $lesson = $this->lessons->find((int) $data['content_id']);
        if ($lesson === null) {
            abort(404);
        }
        $this->assertAccess($lesson);

        $this->lessonFolders->moveLesson(
            (int) auth()->id(),
            (int) $data['content_id'],
            $data['folder_id'] ?? null,
        );

        return response()->json(['message' => 'Dərs daşındı.']);
    }

    /** Dərs qovluğunun sahibliyini yoxla. */
    protected function assertFolderAccess(int $folderId): void
    {
        $folder = $this->lessonFolders->find($folderId);
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

    protected function storeVideo(Request $request): ?string
    {
        $file = $request->file('video');
        if ($file !== null && $file->getSize() > 0) {
            return $file->store(MediaProcessor::VIDEOS_DIR, 'local');
        }

        return null;
    }

    protected function assertAccess(Lesson $lesson): void
    {
        $user = auth()->user();
        if ($user?->isAdmin()) {
            return;
        }
        if ($user === null || $lesson->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
