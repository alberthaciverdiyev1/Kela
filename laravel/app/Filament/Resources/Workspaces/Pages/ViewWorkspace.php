<?php

namespace App\Filament\Resources\Workspaces\Pages;

use App\Application\Library\LibraryService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\Content\Content;
use App\Filament\Resources\Workspaces\WorkspaceResource;
use Filament\Actions\Action;
use Filament\Forms\Components\Select;
use Filament\Forms\Components\TextInput;
use Filament\Notifications\Notification;
use Filament\Resources\Pages\ViewRecord;

class ViewWorkspace extends ViewRecord
{
    protected static string $resource = WorkspaceResource::class;

    protected string $view = 'filament.resources.workspaces.pages.view-workspace';

    public ?int $parentId = null;
    public array $folders = [];
    public array $contents = [];
    public array $breadcrumbs = [];
    public array $students = [];
    public int $studentCount = 0;

    public function mount(int|string $record): void
    {
        parent::mount($record);
        $this->loadWorkspaceData();
    }

    protected function loadWorkspaceData(): void
    {
        $service = app(WorkspaceService::class);
        $workspaceId = (int) $this->record->getKey();
        $teacherId = auth()->id();

        $dir = $service->directory($teacherId, $workspaceId, $this->parentId);
        $this->folders = $dir['folders'];
        $this->contents = $dir['contents'];
        $this->breadcrumbs = $dir['breadcrumbs'];
        $this->students = $service->studentList($teacherId, $workspaceId);
        $this->studentCount = count($this->students);
    }

    public function openFolder(int $nodeId): void
    {
        $this->parentId = $nodeId;
        $this->loadWorkspaceData();
    }

    public function openRoot(): void
    {
        $this->parentId = null;
        $this->loadWorkspaceData();
    }

    public function goToBreadcrumb(int $nodeId): void
    {
        $this->parentId = $nodeId;
        $this->loadWorkspaceData();
    }

    protected function getHeaderActions(): array
    {
        return [
            Action::make('createFolder')
                ->label('Yeni Qovluq')
                ->icon('heroicon-o-folder-plus')
                ->color('gray')
                ->form([
                    TextInput::make('name')->label('Qovluq adı')->required()->maxLength(200),
                ])
                ->action(function (array $data): void {
                    try {
                        app(WorkspaceService::class)->createFolder(
                            auth()->id(),
                            (int) $this->record->getKey(),
                            $data['name'],
                            $this->parentId,
                        );
                        Notification::make()->success()->title('Qovluq yaradıldı')->send();
                        $this->loadWorkspaceData();
                    } catch (\Throwable $e) {
                        Notification::make()->danger()->title($e->getMessage())->send();
                    }
                }),

            Action::make('addContent')
                ->label('Kitabxanadan Məzmun')
                ->icon('heroicon-o-plus')
                ->color('primary')
                ->form([
                    Select::make('content_id')
                        ->label('Məzmun')
                        ->options($this->libraryOptions())
                        ->searchable()
                        ->required(),
                ])
                ->action(function (array $data): void {
                    try {
                        app(WorkspaceService::class)->addContent(
                            auth()->id(),
                            (int) $this->record->getKey(),
                            (int) $data['content_id'],
                            $this->parentId,
                        );
                        Notification::make()->success()->title('Məzmun əlavə edildi')->send();
                        $this->loadWorkspaceData();
                    } catch (\Throwable $e) {
                        Notification::make()->danger()->title($e->getMessage())->send();
                    }
                }),

            Action::make('addStudents')
                ->label('Tələbə Əlavə Et')
                ->icon('heroicon-o-user-plus')
                ->color('gray')
                ->form([
                    Select::make('student_ids')
                        ->label('Tələbələr')
                        ->options($this->availableStudents())
                        ->multiple()
                        ->searchable()
                        ->required(),
                ])
                ->action(function (array $data): void {
                    try {
                        app(WorkspaceService::class)->attachStudents(
                            auth()->id(),
                            (int) $this->record->getKey(),
                            $data['student_ids'] ?? [],
                        );
                        Notification::make()->success()->title('Tələbələr əlavə edildi')->send();
                        $this->loadWorkspaceData();
                    } catch (\Throwable $e) {
                        Notification::make()->danger()->title($e->getMessage())->send();
                    }
                }),

            \Filament\Actions\DeleteAction::make()
                ->label('Workspace Sil')
                ->action(function (): void {
                    app(WorkspaceService::class)->delete(auth()->id(), (int) $this->record->getKey());
                    $this->redirect(WorkspaceResource::getUrl('index'));
                }),
        ];
    }

    public function removeContent(int $nodeId): void
    {
        try {
            app(WorkspaceService::class)->removeContent(
                auth()->id(),
                (int) $this->record->getKey(),
                $nodeId,
            );
            Notification::make()->success()->title('Məzmun çıxarıldı')->send();
            $this->loadWorkspaceData();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function renameNode(int $nodeId): void
    {
        $node = collect($this->folders)->merge($this->contents)->firstWhere('node_id', $nodeId);
        if (! $node) {
            return;
        }
        $this->dispatch('openRenameDialog', nodeId: $nodeId, name: $node['name']);
    }

    public function submitRename(int $nodeId, string $name): void
    {
        try {
            app(WorkspaceService::class)->renameNode(
                auth()->id(),
                (int) $this->record->getKey(),
                $nodeId,
                $name,
            );
            Notification::make()->success()->title('Yeniləndi')->send();
            $this->loadWorkspaceData();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function openMove(int $nodeId): void
    {
        $folders = app(WorkspaceService::class)->folderTree(
            auth()->id(),
            (int) $this->record->getKey(),
            $nodeId,
        );
        $this->dispatch('openMoveDialog', nodeId: $nodeId, folders: $folders);
    }

    public function submitMove(int $nodeId, ?int $parentId): void
    {
        try {
            app(WorkspaceService::class)->moveNode(
                auth()->id(),
                (int) $this->record->getKey(),
                $nodeId,
                $parentId,
            );
            Notification::make()->success()->title('Daşındı')->send();
            $this->loadWorkspaceData();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function deleteNode(int $nodeId): void
    {
        try {
            app(WorkspaceService::class)->deleteNode(
                auth()->id(),
                (int) $this->record->getKey(),
                $nodeId,
            );
            Notification::make()->success()->title('Silindi')->send();
            $this->loadWorkspaceData();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function openContent(int $contentId, int $type): void
    {
        if ($type === Content::TYPE_QUIZ) {
            $this->redirectRoute('filament.admin.resources.quizzes.edit', ['record' => $contentId]);
            return;
        }
        if ($type === Content::TYPE_LESSON) {
            $this->redirectRoute('filament.admin.resources.lessons.view', ['record' => $contentId]);
            return;
        }
        $this->dispatch('openPreview', contentId: $contentId);
    }

    public function detachStudent(int $studentId): void
    {
        try {
            app(WorkspaceService::class)->detachStudent(
                auth()->id(),
                (int) $this->record->getKey(),
                $studentId,
            );
            Notification::make()->success()->title('Tələbə çıxarıldı')->send();
            $this->loadWorkspaceData();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function getTypeLabel(int $type): string
    {
        return match ($type) {
            Content::TYPE_LESSON => 'Dərs',
            Content::TYPE_QUIZ => 'Quiz',
            Content::TYPE_PDF => 'PDF',
            Content::TYPE_VIDEO => 'Video',
            Content::TYPE_LINK => 'Link',
            default => 'Bilinmir',
        };
    }

    protected function libraryOptions(): array
    {
        $teacherId = auth()->id();

        return app(LibraryService::class)->allContentOptions($teacherId);
    }

    protected function availableStudents(): array
    {
        $teacherId = auth()->id();
        $workspaceId = (int) $this->record->getKey();

        return collect(app(WorkspaceService::class)->availableStudents($teacherId, $workspaceId))
            ->pluck('name', 'id')
            ->map(fn ($name) => (string) $name)
            ->all();
    }
}
