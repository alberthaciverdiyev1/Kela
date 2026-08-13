<?php

namespace App\Application\Lesson;

use App\Domain\Content\Content;
use App\Domain\Content\ContentRepository;
use App\Domain\Lesson\Lesson;
use App\Domain\Lesson\LessonRepository;
use App\Domain\User\User;
use App\Infrastructure\Media\MediaProcessor;
use Illuminate\Contracts\Pagination\LengthAwarePaginator;
use Illuminate\Database\Eloquent\Builder;

/**
 * Dərslərlə bağlı tətbiq səviyyəli əməliyyatlar (use cases).
 * Web/API bu servisi çağırır — Content/Lesson modellərinə birbaşa toxunmaz.
 */
class LessonService
{
    public function __construct(
        private readonly LessonRepository $lessons,
        private readonly ContentRepository $contents,
        private readonly MediaProcessor $media,
    ) {
    }

    public function find(int $contentId): ?Lesson
    {
        return $this->lessons->find($contentId);
    }

    /** Dərs yaradır: Content (type=lesson) + Lesson sətri; video varsa meta çıxarır. */
    public function create(int $teacherId, array $data): Lesson
    {
        if (! empty($data['video_path'])) {
            $meta = $this->media->processVideo($data['video_path']);
            $data['thumbnail_path'] = $meta['thumbnail_path'];
            $data['duration_seconds'] = $meta['duration_seconds'];
        }

        $content = $this->contents->create([
            'teacher_id' => $teacherId,
            'workspace_id' => $data['workspace_id'] ?? null,
            'folder_id' => $data['ws_folder_id'] ?? null,
            'title' => $data['title'] ?? '',
            'description' => $data['description'] ?? null,
            'type' => Content::TYPE_LESSON,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);

        return $this->lessons->create([
            'content_id' => $content->id,
            'teacher_id' => $teacherId,
            'folder_id' => $data['folder_id'] ?? null,
            'video_path' => $data['video_path'] ?? null,
            'thumbnail_path' => $data['thumbnail_path'] ?? null,
            'duration_seconds' => (int) ($data['duration_seconds'] ?? 0),
            'is_published' => (bool) ($data['is_published'] ?? false),
            'order_index' => (int) ($data['order_index'] ?? 0),
        ]);
    }

    /** Content alanlarını günceller; video dəyişdiyndə süre/thumbnail yenilənir. */
    public function update(int $contentId, array $data): Lesson
    {
        $lesson = $this->lessons->find($contentId);
        if ($lesson === null) {
            throw new \RuntimeException("Dərs tapılmadı: {$contentId}");
        }

        if (empty($data['video_path'])) {
            $data['thumbnail_path'] = null;
            $data['duration_seconds'] = 0;
        } elseif ($data['video_path'] !== $lesson->video_path) {
            $meta = $this->media->processVideo($data['video_path']);
            $data['thumbnail_path'] = $meta['thumbnail_path'];
            $data['duration_seconds'] = $meta['duration_seconds'];
        }

        if ($lesson->content) {
            $this->contents->update($lesson->content, [
                'workspace_id' => array_key_exists('workspace_id', $data)
                    ? ($data['workspace_id'] ?: null)
                    : $lesson->content->workspace_id,
                'folder_id' => array_key_exists('ws_folder_id', $data)
                    ? ($data['ws_folder_id'] ?: null)
                    : $lesson->content->folder_id,
                'title' => $data['title'] ?? '',
                'description' => $data['description'] ?? null,
                'is_published' => (bool) ($data['is_published'] ?? false),
            ]);
        }

        return $this->lessons->update($lesson, [
            'folder_id' => $data['folder_id'] ?? $lesson->folder_id,
            'video_path' => $data['video_path'] ?? null,
            'thumbnail_path' => $data['thumbnail_path'] ?? null,
            'duration_seconds' => (int) ($data['duration_seconds'] ?? 0),
            'is_published' => (bool) ($data['is_published'] ?? false),
            'order_index' => (int) ($data['order_index'] ?? 0),
        ]);
    }

    /** Dərsi qovluğa daşıyır (null → kökə). */
    public function moveToFolder(int $contentId, ?int $folderId, int $actingUserId): Lesson
    {
        $lesson = $this->lessons->find($contentId);
        if ($lesson === null) {
            throw new \RuntimeException('Dərs tapılmadı.');
        }
        if ($lesson->teacher_id !== $actingUserId) {
            throw new \RuntimeException('Bu dərsi daşımağa icazəniz yoxdur.');
        }

        return $this->lessons->moveToFolder($lesson, $folderId);
    }

    /** Dərs silinir; video faylları da diskdən təmizlənir, Content də silinir. */
    public function delete(int $contentId): void
    {
        $lesson = $this->lessons->find($contentId);
        if ($lesson === null) {
            return;
        }

        // Video + thumbnail fayllarını diskdən sil (yetim fayl qalmasın).
        foreach (array_filter([$lesson->video_path, $lesson->thumbnail_path]) as $path) {
            $full = $this->media->resolveFullPath($path);
            if ($full !== null) {
                @unlink($full);
            }
        }

        $this->lessons->delete($lesson);
    }

    /** Cədvəl üçün istifadəçinin görə biləcəyi dərslərlə məhdud sorğu. */
    public function scopeQueryFor(Builder $query, int $actingUserId): Builder
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->lessons->scopeForUser($query, $actingUserId, $isAdmin);
    }

    /** Cədvəl üçün axtarış + səhifələnmiş dərs siyahısı (array DTO). */
    public function paginate(int $actingUserId, ?string $search = null, int $folderId = 0, int $perPage = 15): LengthAwarePaginator
    {
        $isAdmin = User::find($actingUserId)?->isAdmin() ?? false;

        return $this->lessons
            ->paginateForUser($actingUserId, $isAdmin, $search, $folderId, $perPage)
            ->through(fn (Lesson $lesson): array => [
                'content_id' => (int) $lesson->content_id,
                'folder_id' => $lesson->folder_id ? (int) $lesson->folder_id : null,
                'title' => $lesson->content?->title ?? '',
                'description' => $lesson->content?->description,
                'has_video' => (bool) $lesson->has_video,
                'duration_label' => $lesson->duration_label,
                'is_published' => (bool) $lesson->is_published,
                'order_index' => (int) $lesson->order_index,
                'created_at' => $lesson->created_at?->format('d M Y'),
            ]);
    }

    /** Dərs səhifəsində video player üçün lazım olan bütün məlumat. */
    public function viewerData(int $contentId): array
    {
        $lesson = $this->lessons->find($contentId);
        if ($lesson === null) {
            return [
                'hasVideo' => false,
                'streamUrl' => route('lesson.video.stream', $contentId),
                'thumbUrl' => null,
            ];
        }

        return [
            'hasVideo' => (bool) $lesson->has_video,
            'streamUrl' => route('lesson.video.stream', $contentId),
            'thumbUrl' => $lesson->thumbnail_path ? route('lesson.thumbnail', $contentId) : null,
        ];
    }
}
