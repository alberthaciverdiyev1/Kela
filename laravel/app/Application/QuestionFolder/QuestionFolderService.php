<?php

namespace App\Application\QuestionFolder;

use App\Application\Question\QuestionService;
use App\Domain\QuestionFolder\QuestionFolder;
use App\Domain\QuestionFolder\QuestionFolderRepository;

/**
 * Sual bankı qovluqları: qovluq ağacı + sualları qovluğa yerləşdirmə.
 * Workspace node-larından müstəqildir — hər müəllimin öz qovluq ağacı.
 */
class QuestionFolderService
{
    public function __construct(
        private readonly QuestionFolderRepository $folders,
        private readonly QuestionService $questions,
    ) {
    }

    /** Cari qovluq altındakı qovluqlar + suallar (kataloq görünüşü). */
    public function directory(int $teacherId, ?int $folderId = null): array
    {
        if ($folderId !== null) {
            $this->assertFolderOwner($teacherId, $folderId);
        }

        $folders = $this->folders->foldersFor($teacherId, $folderId);

        return [
            'breadcrumbs' => $this->folders->breadcrumbs($folderId),
            'folders' => $folders->map(fn (QuestionFolder $f) => [
                'id' => (int) $f->id,
                'name' => $f->name,
                'position' => (int) $f->position,
                'question_count' => (int) $f->questions()->count(),
            ])->values()->all(),
            'questions' => $this->questions->listForTeacher($teacherId, null, $folderId ?? 0),
        ];
    }

    public function find(int $folderId): ?QuestionFolder
    {
        return $this->folders->find($folderId);
    }

    public function createFolder(int $teacherId, string $name, ?int $parentId = null): QuestionFolder
    {
        if ($parentId !== null) {
            $this->assertFolderOwner($teacherId, $parentId);
        }
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Qovluq adı boş ola bilməz.');
        }

        return $this->folders->create($teacherId, $name, $parentId);
    }

    public function renameFolder(int $teacherId, int $folderId, string $name): void
    {
        $folder = $this->assertFolderOwner($teacherId, $folderId);
        $name = trim($name);
        if ($name === '') {
            throw new \InvalidArgumentException('Qovluq adı boş ola bilməz.');
        }

        $this->folders->update($folder, ['name' => $name]);
    }

    public function moveFolder(int $teacherId, int $folderId, ?int $newParentId): void
    {
        $folder = $this->assertFolderOwner($teacherId, $folderId);

        if ($newParentId !== null) {
            $parent = $this->assertFolderOwner($teacherId, $newParentId);
            if (in_array($parent->id, $this->folders->descendantIds($folderId), true)) {
                throw new \RuntimeException('Qovluq öz daxilinə daşına bilməz.');
            }
        }

        $this->folders->update($folder, ['parent_id' => $newParentId]);
    }

    public function deleteFolder(int $teacherId, int $folderId): void
    {
        $this->assertFolderOwner($teacherId, $folderId);
        $this->folders->deleteTree($folderId);
    }

    /** Move dropdown üçün teacher-ın bütün qovluq ağacı. */
    public function folderTree(int $teacherId, ?int $excludeFolderId = null): array
    {
        $folders = $this->folders->allFoldersFor($teacherId);
        $byParent = $folders->groupBy(fn (QuestionFolder $f) => $f->parent_id ?? 0);

        $result = [];
        $walk = function (int $parentKey, int $depth) use (&$walk, &$result, $byParent, $excludeFolderId): void {
            foreach ($byParent->get($parentKey, collect()) as $folder) {
                if ($folder->id === $excludeFolderId) {
                    continue;
                }
                $result[] = ['id' => (int) $folder->id, 'name' => $folder->name, 'depth' => $depth];
                $walk($folder->id, $depth + 1);
            }
        };

        $walk(0, 0);

        return $result;
    }

    /** Sualı qovluğa daşıyır (null → kökə). */
    public function moveQuestion(int $teacherId, int $questionId, ?int $folderId): void
    {
        if ($folderId !== null) {
            $this->assertFolderOwner($teacherId, $folderId);
        }

        $this->questions->moveToFolder($questionId, $folderId, $teacherId);
    }

    /** Sualı yaradarkən verilən qovluq sahibə aiddirsə dəyəri qaytarır. */
    public function resolveFolderFor(int $teacherId, ?int $folderId): ?int
    {
        if ($folderId === null) {
            return null;
        }

        return $this->assertFolderOwner($teacherId, $folderId)->id;
    }

    protected function assertFolderOwner(int $teacherId, int $folderId): QuestionFolder
    {
        $folder = $this->folders->find($folderId);
        if ($folder === null || $folder->teacher_id !== $teacherId) {
            throw new \RuntimeException('Qovluq tapılmadı.');
        }

        return $folder;
    }
}
