<?php

namespace App\Filament\Resources\Quizzes\Tables;

use App\Application\Library\LibraryService;
use App\Domain\Quiz\Quiz;
use Filament\Actions\EditAction;
use Filament\Tables\Columns\IconColumn;
use Filament\Tables\Columns\TextColumn;
use Filament\Tables\Table;
use Illuminate\Database\Eloquent\Builder;

class QuizzesTable
{
    public static function configure(Table $table): Table
    {
        return $table
            ->columns([
                TextColumn::make('content.title')
                    ->label('Başlıq')
                    ->searchable()
                    ->weight('medium')
                    ->description(fn (Quiz $record): ?string => $record->content?->description),

                TextColumn::make('questions_count')
                    ->label('Sual sayı')
                    ->counts('questions')
                    ->badge()
                    ->color('gray'),

                IconColumn::make('is_published')
                    ->label('Yayımlandı')
                    ->boolean()
                    ->trueIcon('heroicon-o-check-circle')
                    ->falseIcon('heroicon-o-x-circle')
                    ->trueColor('success')
                    ->falseColor('danger'),

                TextColumn::make('created_at')
                    ->label('Yaradılıb')
                    ->dateTime('d M Y')
                    ->sortable(),
            ])
            ->defaultSort('created_at', 'desc')
            ->filters([])
            ->recordActions([
                EditAction::make()->label('Redaktə et'),
            ])
            ->modifyQueryUsing(function (Builder $query): Builder {
                $service = app(LibraryService::class);
                $teacherId = auth()->id();

                return $query
                    ->when(
                        ! auth()->user()?->isAdmin() ?? false,
                        fn (Builder $q): Builder => $q->where('teacher_id', $teacherId),
                    )
                    ->with(['content', 'questions']);
            });
    }
}
