<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Lesson\LessonService;
use App\Application\LessonFolder\LessonFolderService;
use App\Domain\Lesson\Lesson;
use App\Infrastructure\Media\MediaProcessor;
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
        ]);
    }

    public function store(Request $request): RedirectResponse
    {
        $data = $this->validated($request);
        $data['folder_id'] = $this->lessonFolders->resolveFolderFor((int) auth()->id(), $data['folder_id'] ?? null);

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
                'created_at' => $model->created_at?->format('d M Y H:i'),
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
        ]);
    }

    public function update(Request $request, int $lesson): RedirectResponse
    {
        $model = $this->lessons->find($lesson);
        if ($model === null) {
            abort(404);
        }
        $this->assertAccess($model);

        $data = $this->validated($request);
        $data['folder_id'] = $this->lessonFolders->resolveFolderFor((int) auth()->id(), $data['folder_id'] ?? null);

        $newPath = $this->storeVideo($request);
        $data['video_path'] = $newPath ?? $model->video_path;

        $this->lessons->update($lesson, $data);

        return redirect()->route('teacher.lessons.index')->with('success', 'Dərs yeniləndi.');
    }

    public function destroy(int $lesson): RedirectResponse
    {
        $model = $this->lessons->find($lesson);
        if ($model === null) {
            abort(404);
        }
        $this->assertAccess($model);

        $this->lessons->delete($lesson);

        return redirect()->route('teacher.lessons.index')->with('success', 'Dərs silindi.');
    }

    protected function validated(Request $request): array
    {
        $data = $request->validate([
            'title' => ['required', 'string', 'max:255'],
            'description' => ['nullable', 'string'],
            'video' => ['nullable', 'file', 'max:524288', 'mimetypes:video/mp4,video/webm,video/ogg,video/quicktime,video/x-m4v,video/x-matroska,video/x-msvideo,video/mpeg'],
            'is_published' => ['nullable', 'boolean'],
            'order_index' => ['integer', 'min:0'],
            'folder_id' => ['nullable', 'integer'],
        ]);

        $data['is_published'] = $request->boolean('is_published');
        $data['order_index'] = (int) $data['order_index'];

        return $data;
    }

    protected function storeVideo(Request $request): ?string
    {
        if ($request->hasFile('video')) {
            return $request->file('video')->store(MediaProcessor::VIDEOS_DIR, 'local');
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
