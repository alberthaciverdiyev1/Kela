<?php

namespace App\Filament\Resources\Workspaces\Tables;

use App\Application\Workspace\WorkspaceService;
use App\Domain\Workspace\Workspace;
use Filament\Actions\EditAction;
use Filament\Actions\ViewAction;
use Filament\Tables\Columns\TextColumn;
use Filament\Tables\Table;
use Illuminate\Database\Eloquent\Builder;

class WorkspacesTable
{
    public static function configure(Table $table): Table
    {
        return $table
            ->columns([
                TextColumn::make('name')
                    ->label('Ad')
                    ->searchable()
                    ->weight('medium')
                    ->description(fn (Workspace $record): string => "Yaradıldı: {$record->created_at?->toDateString()}"),

                TextColumn::make('students_count')
                    ->label('Tələbə')
                    ->counts('students')
                    ->badge()
                    ->color('gray'),
            ])
            ->defaultSort('name')
            ->recordActions([
                ViewAction::make()->label('Aç'),
                EditAction::make(),
            ])
            ->modifyQueryUsing(function (Builder $query): Builder {
                $teacherId = auth()->id();
                $isAdmin = auth()->user()?->isAdmin() ?? false;

                return $query
                    ->when(! $isAdmin, fn (Builder $q): Builder => $q->where('teacher_id', $teacherId))
                    ->withCount('students');
            });
    }
}
