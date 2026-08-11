<?php

namespace App\Providers\Filament;

use Filament\Http\Middleware\Authenticate;
use Filament\Http\Middleware\AuthenticateSession;
use Filament\Http\Middleware\DisableBladeIconComponents;
use Filament\Http\Middleware\DispatchServingFilamentEvent;
use App\Domain\User\User;
use Filament\Pages\Dashboard;
use Filament\Panel;
use Filament\PanelProvider;
use Filament\Support\Colors\Color;
use Filament\Widgets\AccountWidget;
use MmesDesign\FilamentFileManager\FileManagerPlugin;
use Illuminate\Cookie\Middleware\AddQueuedCookiesToResponse;
use Illuminate\Cookie\Middleware\EncryptCookies;
use Illuminate\Foundation\Http\Middleware\PreventRequestForgery;
use Illuminate\Routing\Middleware\SubstituteBindings;
use Illuminate\Session\Middleware\StartSession;
use Illuminate\View\Middleware\ShareErrorsFromSession;

class AdminPanelProvider extends PanelProvider
{
    public function panel(Panel $panel): Panel
    {
        return $panel
            ->default()
            ->id('admin')
            ->path('admin')
            ->login()
            ->colors([
                'primary' => Color::Sky,
            ])
            ->brandName('Kela')
            ->discoverResources(in: app_path('Filament/Resources'), for: 'App\Filament\Resources')
            ->discoverPages(in: app_path('Filament/Pages'), for: 'App\Filament\Pages')
            ->pages([
                Dashboard::class,
            ])
            ->discoverWidgets(in: app_path('Filament/Widgets'), for: 'App\Filament\Widgets')
            ->widgets([
                AccountWidget::class,
            ])
            ->plugins([
                FileManagerPlugin::make()
                    ->defaultDisk('local')
                    ->navigationGroup('Kütüphane')
                    ->navigationIcon('heroicon-o-folder-open')
                    ->navigationSort(3)
                    ->canAccess(fn (): bool => auth()->user()?->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]) ?? false)
                    ->canUpload(fn (): bool => auth()->user()?->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]) ?? false)
                    ->canDelete(fn (): bool => auth()->user()?->isAdmin() ?? false)
                    ->canRename(fn (): bool => auth()->user()?->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]) ?? false)
                    ->canMove(fn (): bool => auth()->user()?->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]) ?? false)
                    ->canDownload(fn (): bool => auth()->user()?->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]) ?? false)
                    ->canCreateFolder(fn (): bool => auth()->user()?->hasAnyRole([User::ROLE_ADMIN, User::ROLE_TEACHER]) ?? false),
            ])
            ->middleware([
                EncryptCookies::class,
                AddQueuedCookiesToResponse::class,
                StartSession::class,
                AuthenticateSession::class,
                ShareErrorsFromSession::class,
                PreventRequestForgery::class,
                SubstituteBindings::class,
                DisableBladeIconComponents::class,
                DispatchServingFilamentEvent::class,
            ])
            ->authMiddleware([
                Authenticate::class,
            ]);
    }
}
