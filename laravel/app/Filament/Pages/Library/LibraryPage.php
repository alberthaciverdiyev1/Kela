<?php

namespace App\Filament\Pages\Library;

use App\Application\Library\LibraryService;
use App\Domain\Content\Content;
use Filament\Actions\Action;
use Filament\Forms\Components\Select;
use Filament\Forms\Components\Textarea;
use Filament\Forms\Components\TextInput;
use Filament\Forms\Components\Toggle;
use Filament\Notifications\Notification;
use Filament\Pages\Page;
use Illuminate\Support\HtmlString;

/**
 * Kitabxana — Dərs/Quiz məzmunu üçün file-manager tərzi node ağacı.
 * Bütün əməliyyatlar LibraryService üzərindən aparılır; modellərə toxunulmur.
 */
class LibraryPage extends Page
{
    protected static ?string $slug = 'library';

    protected string $view = 'filament.pages.library.library-page';

    protected static string | \BackedEnum | null $navigationIcon = 'heroicon-o-folder-open';
    protected static string | \UnitEnum | null $navigationGroup = 'Kütüphane';
    protected static ?string $navigationLabel = 'Kütüphane';
    protected static ?string $title = 'Kütüphane';
    protected static ?int $navigationSort = 4;

    // --- State ---
    public ?int $parentId = null;
    public ?int $type = null;
    public array $folders = [];
    public array $contents = [];
    public array $breadcrumbs = [];
    public array $counts = [];
    public ?string $currentPath = 'Kök';

    public function mount(): void
    {
        $this->loadDirectory();
    }

    public function setType(?int $type): void
    {
        $this->type = $type;
        $this->parentId = null;
        $this->loadDirectory();
    }

    public function updatedParentId(): void
    {
        $this->loadDirectory();
    }

    protected function loadDirectory(): void
    {
        $service = app(LibraryService::class);
        $teacherId = auth()->id();

        $dir = $service->directory($teacherId, $this->parentId, $this->type);
        $this->folders = $dir['folders'];
        $this->contents = $dir['contents'];
        $this->breadcrumbs = $dir['breadcrumbs'];
        $this->counts = $service->counts($teacherId);

        $path = array_column($this->breadcrumbs, 'name');
        $this->currentPath = $path === [] ? 'Kök' : implode(' / ', $path);
    }

    public function openFolder(int $nodeId): void
    {
        $this->parentId = $nodeId;
        $this->loadDirectory();
    }

    public function openRoot(): void
    {
        $this->parentId = null;
        $this->loadDirectory();
    }

    public function goToBreadcrumb(int $nodeId): void
    {
        $this->parentId = $nodeId;
        $this->loadDirectory();
    }

    // --- Actions (header) ---

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
                        app(LibraryService::class)->createFolder(auth()->id(), $data['name'], $this->parentId);
                        Notification::make()->success()->title('Qovluq yaradıldı')->send();
                        $this->loadDirectory();
                    } catch (\Throwable $e) {
                        Notification::make()->danger()->title($e->getMessage())->send();
                    }
                }),

            Action::make('createContent')
                ->label('Yeni Məzmun')
                ->icon('heroicon-o-plus')
                ->color('primary')
                ->form([
                    TextInput::make('title')->label('Başlıq')->required()->maxLength(200),
                    Select::make('type')
                        ->label('Tip')
                        ->options([
                            Content::TYPE_LESSON => 'Dərs',
                            Content::TYPE_QUIZ => 'Quiz',
                            Content::TYPE_PDF => 'PDF',
                            Content::TYPE_VIDEO => 'Video',
                            Content::TYPE_LINK => 'Link',
                        ])
                        ->default($this->type ?? Content::TYPE_LESSON)
                        ->required(),
                    Textarea::make('description')->label('Təsvir')->rows(3),
                    TextInput::make('url')->label('URL (Link/Video)')->url(),
                    Toggle::make('is_published')->label('Yayımlandı')->default(false),
                ])
                ->action(function (array $data): void {
                    try {
                        $contentId = app(LibraryService::class)->createContent(
                            auth()->id(),
                            $data,
                            $this->parentId,
                        );
                        $this->loadDirectory();

                        if ((int) ($data['type'] ?? -1) === Content::TYPE_QUIZ) {
                            Notification::make()->success()->title('Quiz yaradıldı')
                                ->body('Sual əlavə etmək üçün quiz redaktorunu açın.')
                                ->send();
                            $this->redirectRoute('filament.admin.resources.quizzes.edit', ['record' => $contentId]);
                            return;
                        }
                        if ((int) ($data['type'] ?? -1) === Content::TYPE_LESSON) {
                            $this->redirectRoute('filament.admin.resources.lessons.edit', ['record' => $contentId]);
                            return;
                        }
                        Notification::make()->success()->title('Məzmun yaradıldı')->send();
                    } catch (\Throwable $e) {
                        Notification::make()->danger()->title($e->getMessage())->send();
                    }
                }),
        ];
    }

    // --- Row actions (rendered in the blade view) ---

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
            app(LibraryService::class)->renameNode(auth()->id(), $nodeId, $name);
            Notification::make()->success()->title('Yeniləndi')->send();
            $this->loadDirectory();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function togglePublish(int $contentId): void
    {
        try {
            app(LibraryService::class)->setPublished($contentId, ! $this->isPublished($contentId));
            Notification::make()->success()->title('Yayım statusu dəyişdirildi')->send();
            $this->loadDirectory();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function deleteNode(int $nodeId): void
    {
        try {
            app(LibraryService::class)->deleteNode(auth()->id(), $nodeId);
            Notification::make()->success()->title('Silindi')->send();
            $this->loadDirectory();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function openMove(int $nodeId): void
    {
        $folders = app(LibraryService::class)->folderTree(auth()->id(), $nodeId);
        $this->dispatch('openMoveDialog', nodeId: $nodeId, folders: $folders);
    }

    public function submitMove(int $nodeId, ?int $parentId): void
    {
        try {
            app(LibraryService::class)->moveNode(auth()->id(), $nodeId, $parentId);
            Notification::make()->success()->title('Daşındı')->send();
            $this->loadDirectory();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    public function editContent(int $contentId, int $type): void
    {
        if ($type === Content::TYPE_QUIZ) {
            $this->redirectRoute('filament.admin.resources.quizzes.edit', ['record' => $contentId]);
            return;
        }
        if ($type === Content::TYPE_LESSON) {
            $this->redirectRoute('filament.admin.resources.lessons.edit', ['record' => $contentId]);
            return;
        }
        // PDF/Video/Link — bu hələlik məzmun önizləməsi kimi açılır.
        $this->dispatch('openPreview', contentId: $contentId);
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

    protected function isPublished(int $contentId): bool
    {
        $content = collect($this->contents)->firstWhere('content_id', $contentId);

        return $content ? (bool) $content['is_published'] : false;
    }

    public static function canAccess(): bool
    {
        $user = auth()->user();

        return $user !== null && $user->hasAnyRole([
            \App\Domain\User\User::ROLE_ADMIN,
            \App\Domain\User\User::ROLE_TEACHER,
        ]);
    }
}
