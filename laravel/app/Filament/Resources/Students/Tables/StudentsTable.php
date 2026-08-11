<?php

namespace App\Filament\Resources\Students\Tables;

use App\Application\Student\StudentService;
use App\Domain\User\User;
use Filament\Actions\BulkActionGroup;
use Filament\Actions\DeleteBulkAction;
use Filament\Actions\EditAction;
use Filament\Actions\ViewAction;
use Filament\Tables\Columns\TextColumn;
use Filament\Tables\Table;
use Illuminate\Database\Eloquent\Builder;

class StudentsTable
{
    public static function configure(Table $table): Table
    {
        return $table
            ->columns([
                TextColumn::make('full_name')
                    ->label('Ad Soyad')
                    ->searchable(['first_name', 'last_name'])
                    ->weight('medium'),

                TextColumn::make('email')
                    ->label('E-poçt')
                    ->searchable()
                    ->icon('heroicon-o-envelope'),

                TextColumn::make('studentProfile.city.name')
                    ->label('Şəhər')
                    ->placeholder('-'),

                TextColumn::make('studentProfile.birth_date')
                    ->label('Doğum')
                    ->date('d M Y')
                    ->placeholder('-')
                    ->toggleable(isToggledHiddenByDefault: true),

                TextColumn::make('status')
                    ->label('Status')
                    ->badge()
                    ->formatStateUsing(fn (int $state): string => match ($state) {
                        1 => 'Aktiv',
                        2 => 'Deaktiv',
                        3 => 'Dayandırılmış',
                        default => (string) $state,
                    })
                    ->color(fn (int $state): string => match ($state) {
                        1 => 'success',
                        2 => 'warning',
                        3 => 'danger',
                        default => 'gray',
                    }),

                TextColumn::make('created_at')
                    ->label('Qeydiyyat')
                    ->dateTime('d M Y')
                    ->sortable()
                    ->toggleable(isToggledHiddenByDefault: true),
            ])
            ->defaultSort('first_name')
            ->recordActions([
                ViewAction::make(),
                EditAction::make(),
            ])
            ->toolbarActions([
                BulkActionGroup::make([
                    DeleteBulkAction::make(),
                ]),
            ])
            ->modifyQueryUsing(fn (Builder $query): Builder => app(StudentService::class)->scopeQueryFor($query));
    }
}
