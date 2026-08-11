<?php

namespace App\Filament\Resources\Lessons\Tables;

use App\Application\Lesson\LessonService;
use App\Domain\Lesson\Lesson;
use Filament\Actions\BulkActionGroup;
use Filament\Actions\DeleteBulkAction;
use Filament\Actions\EditAction;
use Filament\Actions\ViewAction;
use Filament\Tables\Columns\IconColumn;
use Filament\Tables\Columns\TextColumn;
use Filament\Tables\Table;
use Illuminate\Database\Eloquent\Builder;

class LessonsTable
{
    public static function configure(Table $table): Table
    {
        return $table
            ->columns([
                TextColumn::make('content.title')
                    ->label('Başlıq')
                    ->searchable()
                    ->weight('medium')
                    ->description(fn (Lesson $record): ?string => $record->content?->description),

                IconColumn::make('has_video')
                    ->label('Video')
                    ->icon(fn (Lesson $record): ?string => $record->has_video ? 'heroicon-o-video-camera' : null)
                    ->color(fn (Lesson $record): ?string => $record->has_video ? 'success' : 'gray')
                    ->tooltip(fn (Lesson $record): ?string => $record->has_video ? 'Video yüklənib' : 'Video yoxdur'),

                TextColumn::make('duration_label')
                    ->label('Müddət')
                    ->badge()
                    ->color('gray'),

                IconColumn::make('is_published')
                    ->label('Yayımlandı')
                    ->boolean()
                    ->trueIcon('heroicon-o-check-circle')
                    ->falseIcon('heroicon-o-x-circle')
                    ->trueColor('success')
                    ->falseColor('danger'),

                TextColumn::make('order_index')
                    ->label('Sıra')
                    ->sortable(),

                TextColumn::make('created_at')
                    ->label('Yaradılıb')
                    ->dateTime('d M Y')
                    ->sortable()
                    ->toggleable(isToggledHiddenByDefault: true),
            ])
            ->defaultSort('order_index')
            ->filters([
                //
            ])
            ->recordActions([
                ViewAction::make(),
                EditAction::make(),
            ])
            ->toolbarActions([
                BulkActionGroup::make([
                    DeleteBulkAction::make(),
                ]),
            ])
            ->modifyQueryUsing(function (Builder $query): Builder {
                return app(LessonService::class)->scopeQueryFor($query, auth()->id());
            });
    }
}
