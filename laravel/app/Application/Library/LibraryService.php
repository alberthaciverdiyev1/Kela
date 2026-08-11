<?php

namespace App\Application\Library;

use App\Domain\Content\Content;
use App\Domain\Content\ContentRepository;
use App\Domain\Lesson\LessonRepository;
use App\Domain\Node\Node;
use App\Domain\Node\NodeRepository;
use App\Domain\Quiz\QuizRepository;
use Illuminate\Support\Collection;

/**
 * Kütüphane (kitabxana) node ağacı — file-manager tərzi əməliyyatlar.
 * Filament bu servisi çağırır; modellərə birbaşa toxunmaz.
 */
class LibraryService
{
    public function __construct(
        private readonly NodeRepository $nodes,
        private readonly ContentRepository $contents,
        private readonly LessonRepository $lessons,
        private readonly QuizRepository $quizzes,
    ) {
    }

    /** Hal-hazırki qovluğun direktoriyası: breadcrumb + qovluqlar + məzmunlar. */
    public function directory(int $teacherId, ?int $parentId = null, ?int $type = null): array
    {
        $this->assertLibraryParent($teacherId, $parentId);

        $folders = $this->nodes->libraryFolders($teacherId, $parentId);
        $contents = $this->nodes->libraryContents($teacherId, $parentId, $type);

        return [
            'breadcrumbs' => $this->nodes->breadcrumbs($parentId),
            'folders' => $folders->map(fn (Node $n) => $this->folderData($n))->values()->all(),
            'contents' => $contents->map(fn (Node $n) => $this->contentData($n))->values()->all(),
        ];
    }

    /** Sekme sayaçları: [0 => lessonCount, 1 => quizCount, ...]. */
    public function counts(int $teacherId): array
    {
        return $this->contents->countByType($teacherId);
    }

    public function createFolder(int $teacherId, string $name, ?int $parentId = null): Node
    {
        $this->assertLibraryParent($teacherId, $parentId);
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Qovluq adı boş ola bilməz.');
        }

        return $this->nodes->createFolder($teacherId, $name, $parentId);
    }

    /**
     * Yeni məzmun yaradır: Content (+ Dərs/Quiz sətri) + Node. Content id qaytarır.
     */
    public function createContent(int $teacherId, array $data, ?int $parentId = null): int
    {
        $this->assertLibraryParent($teacherId, $parentId);

        $title = trim($data['title'] ?? '');
        if ($title === '') {
            throw new \InvalidArgumentException('Məzmun başlığı boş ola bilməz.');
        }

        $type = (int) ($data['type'] ?? Content::TYPE_LESSON);
        if (! in_array($type, Content::ALL_TYPES, true)) {
            throw new \InvalidArgumentException('Naməlum məzmun tipi.');
        }

        $content = $this->contents->create([
            'teacher_id' => $teacherId,
            'title' => $title,
            'description' => $data['description'] ?? null,
            'type' => $type,
            'url' => $data['url'] ?? null,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);

        if ($type === Content::TYPE_LESSON) {
            $this->lessons->create([
                'content_id' => $content->id,
                'teacher_id' => $teacherId,
                'is_published' => (bool) ($data['is_published'] ?? false),
                'order_index' => (int) ($data['order_index'] ?? 0),
            ]);
        } elseif ($type === Content::TYPE_QUIZ) {
            $this->quizzes->create($content->id, $teacherId, [
                'title' => $title,
                'description' => $data['description'] ?? null,
                'is_published' => (bool) ($data['is_published'] ?? false),
            ]);
        }

        $this->nodes->createContentNode($teacherId, $content->id, $title, $parentId);

        return $content->id;
    }

    public function updateContent(int $contentId, array $data): Content
    {
        $content = $this->contents->find($contentId);
        if ($content === null) {
            throw new \RuntimeException("Məzmun tapılmadı: {$contentId}");
        }

        $content = $this->contents->update($content, [
            'title' => $data['title'] ?? $content->title,
            'description' => $data['description'] ?? $content->description,
            'url' => array_key_exists('url', $data) ? $data['url'] : $content->url,
            'is_published' => array_key_exists('is_published', $data)
                ? (bool) $data['is_published']
                : $content->is_published,
        ]);

        // Bağlı Dərs/Quiz sətirlərini də sinxron saxla.
        if ($content->isLesson() && $content->lesson) {
            $this->lessons->update($content->lesson, [
                'title' => $data['title'] ?? $content->title,
                'description' => $data['description'] ?? $content->description,
                'is_published' => $content->is_published,
            ]);
        } elseif ($content->isQuiz() && $content->quiz) {
            $this->quizzes->update($content->quiz, [
                'title' => $data['title'] ?? $content->title,
                'description' => $data['description'] ?? $content->description,
                'is_published' => $content->is_published,
            ]);
        }

        return $content;
    }

    public function setPublished(int $contentId, bool $published): void
    {
        $content = $this->contents->find($contentId);
        if ($content === null) {
            return;
        }

        $this->contents->update($content, ['is_published' => $published]);

        if ($content->isLesson() && $content->lesson) {
            $this->lessons->update($content->lesson, ['is_published' => $published]);
        } elseif ($content->isQuiz() && $content->quiz) {
            $this->quizzes->update($content->quiz, ['is_published' => $published]);
        }
    }

    public function findContent(int $contentId): ?Content
    {
        return $this->contents->find($contentId);
    }

    /** Məzmunu silir: node referansları + bağlı Dərs/Quiz sətri + Content (soft). */
    public function deleteContent(int $contentId): void
    {
        $content = $this->contents->find($contentId);
        if ($content === null) {
            return;
        }

        foreach ($content->nodes()->get() as $node) {
            $this->nodes->delete($node);
        }

        if ($content->isLesson() && $content->lesson) {
            // Lesson::deleting hadisəsi Content-i də silir.
            $this->lessons->delete($content->lesson);
            if (! $content->trashed()) {
                $this->contents->delete($content);
            }
        } elseif ($content->isQuiz() && $content->quiz) {
            $this->quizzes->delete($content->quiz);
            $this->contents->delete($content);
        } else {
            $this->contents->delete($content);
        }
    }

    /** Node adını yeniləyir; məzmun node-u üçün Content başlığını da sinxron edir. */
    public function renameNode(int $teacherId, int $nodeId, string $name): void
    {
        $node = $this->nodes->find($nodeId);
        if ($node === null || $node->teacher_id !== $teacherId || $node->workspace_id !== null) {
            throw new \RuntimeException('Qovluq tapılmadı.');
        }

        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Ad boş ola bilməz.');
        }

        $this->nodes->update($node, ['name' => $name]);

        if ($node->isContent() && $node->content) {
            $this->contents->update($node->content, ['title' => $name]);
            if ($node->content->lesson) {
                $this->lessons->update($node->content->lesson, ['title' => $name]);
            }
            if ($node->content->quiz) {
                $this->quizzes->update($node->content->quiz, ['title' => $name]);
            }
        }
    }

    /** Node-u başqa qovluğa daşıyır (döngü yoxlaması ilə). */
    public function moveNode(int $teacherId, int $nodeId, ?int $newParentId): void
    {
        $node = $this->nodes->find($nodeId);
        if ($node === null || $node->teacher_id !== $teacherId || $node->workspace_id !== null) {
            throw new \RuntimeException('Element tapılmadı.');
        }

        if ($newParentId !== null) {
            $parent = $this->nodes->find($newParentId);
            if ($parent === null || $parent->teacher_id !== $teacherId || $parent->workspace_id !== null) {
                throw new \RuntimeException('Hədəf qovluq tapılmadı.');
            }
            if (! $parent->isFolder()) {
                throw new \RuntimeException('Yalnız qovluğa daşına bilər.');
            }
            if ($node->isFolder() && $this->isSelfOrDescendant($parent, $nodeId)) {
                throw new \RuntimeException('Qovluq öz daxilinə daşına bilməz.');
            }
        }

        $this->nodes->update($node, ['parent_id' => $newParentId]);
    }

    /** Qovluğu və ya məzmunu silir (kitabxana konteksti). */
    public function deleteNode(int $teacherId, int $nodeId): void
    {
        $node = $this->nodes->find($nodeId);
        if ($node === null || $node->teacher_id !== $teacherId || $node->workspace_id !== null) {
            throw new \RuntimeException('Element tapılmadı.');
        }

        if ($node->isContent()) {
            $this->deleteContent($node->content_id);
            return;
        }

        // Qovluq: bütün alt məzmunları topla, node ağacını sil, sonra məzmunları sil.
        $contentIds = $this->collectContentIds($nodeId);
        $this->nodes->deleteTree($nodeId);

        foreach ($contentIds as $contentId) {
            $this->deleteContent($contentId);
        }
    }

    /** Workspace-a əlavə edilə bilən məzmunlar: [contentId => title]. */
    public function allContentOptions(int $teacherId): array
    {
        return $this->contents->allForTeacher($teacherId);
    }

    /** Move dropdown üçün bütün qovluqlar (exclude-dan törəmələr xaric). */
    public function folderTree(int $teacherId, ?int $excludeNodeId = null): array
    {
        $folders = $this->nodes->allLibraryFolders($teacherId);
        $byParent = $folders->groupBy(fn (Node $n) => $n->parent_id ?? 0);

        $result = [];
        $walk = function (int $parentKey, int $depth) use (&$walk, &$result, $byParent, $excludeNodeId): void {
            foreach ($byParent->get($parentKey, collect()) as $folder) {
                if ($folder->id === $excludeNodeId) {
                    continue;
                }
                $result[] = ['id' => $folder->id, 'name' => $folder->name, 'depth' => $depth];
                if ($folder->id !== $excludeNodeId) {
                    $walk($folder->id, $depth + 1);
                }
            }
        };

        $walk(0, 0);

        return $result;
    }

    // --- DTO helpers ---

    protected function folderData(Node $node): array
    {
        return [
            'node_id' => $node->id,
            'name' => $node->name,
            'position' => $node->position,
            'parent_id' => $node->parent_id,
        ];
    }

    protected function contentData(Node $node): array
    {
        $content = $node->content;
        if ($content === null) {
            return [
                'node_id' => $node->id,
                'content_id' => null,
                'title' => $node->name,
                'description' => null,
                'type' => Content::TYPE_LINK,
                'type_label' => 'Unknown',
                'is_published' => false,
                'has_video' => false,
                'duration_label' => null,
                'url' => null,
                'question_count' => 0,
            ];
        }

        $lesson = $content->lesson;
        $quiz = $content->quiz;

        return [
            'node_id' => $node->id,
            'content_id' => $content->id,
            'title' => $content->title,
            'description' => $content->description,
            'type' => $content->type,
            'type_label' => $content->typeLabel(),
            'is_published' => $content->is_published,
            'has_video' => $lesson?->has_video ?? false,
            'duration_label' => $lesson?->duration_label ?? null,
            'url' => $content->url,
            'question_count' => (int) ($quiz?->questions_count ?? 0),
        ];
    }

    protected function assertLibraryParent(int $teacherId, ?int $parentId): void
    {
        if ($parentId === null) {
            return;
        }

        $parent = $this->nodes->find($parentId);
        if ($parent === null || $parent->teacher_id !== $teacherId || $parent->workspace_id !== null) {
            throw new \RuntimeException('Qovluq tapılmadı.');
        }
        if (! $parent->isFolder()) {
            throw new \RuntimeException('Məzmun altında qovluq aça bilməz.');
        }
    }

    protected function isSelfOrDescendant(Node $node, int $ancestorId): bool
    {
        $current = $node;
        while ($current->parent_id !== null) {
            if ($current->parent_id === $ancestorId) {
                return true;
            }
            $current = $this->nodes->find($current->parent_id);
            if ($current === null) {
                return false;
            }
        }

        return false;
    }

    protected function collectContentIds(int $nodeId): array
    {
        $ids = [];
        $queue = [$nodeId];

        while ($queue !== []) {
            $id = array_shift($queue);
            foreach ($this->nodes->children($id) as $child) {
                if ($child->isContent() && $child->content_id) {
                    $ids[] = $child->content_id;
                }
                if ($child->isFolder()) {
                    $queue[] = $child->id;
                }
            }
        }

        return array_values(array_unique($ids));
    }
}
