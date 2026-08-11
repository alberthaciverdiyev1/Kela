<?php

namespace App\Filament\Resources\Lessons\Schemas;

use App\Domain\Lesson\Lesson;
use Filament\Infolists\Components\IconEntry;
use Filament\Infolists\Components\TextEntry;
use Filament\Infolists\Components\ViewEntry;
use Filament\Schemas\Schema;

class LessonInfolist
{
    public static function configure(Schema $schema): Schema
    {
        return $schema
            ->columns(2)
            ->components([
                ViewEntry::make('video_player')
                    ->view('filament.resources.lessons.video-player')
                    ->viewData(function (Lesson $record): array {
                        return [
                            'hasVideo' => $record->has_video,
                            'streamUrl' => route('lesson.video.stream', $record->getKey()),
                            'thumbUrl' => $record->thumbnail_path
                                ? route('lesson.thumbnail', $record->getKey())
                                : null,
                        ];
                    })
                    ->columnSpanFull(),

                TextEntry::make('content.title')
                    ->label('Başlıq')
                    ->weight('medium'),

                IconEntry::make('is_published')
                    ->label('Yayımlandı')
                    ->boolean()
                    ->trueIcon('heroicon-o-check-circle')
                    ->falseIcon('heroicon-o-x-circle')
                    ->trueColor('success')
                    ->falseColor('danger'),

                TextEntry::make('duration_label')
                    ->label('Müddət')
                    ->badge()
                    ->color('gray'),

                TextEntry::make('order_index')
                    ->label('Sıra')
                    ->numeric(),

                TextEntry::make('content.description')
                    ->label('Təsvir')
                    ->placeholder('-')
                    ->columnSpanFull(),

                TextEntry::make('created_at')
                    ->label('Yaradılıb')
                    ->dateTime('d M Y H:i')
                    ->placeholder('-'),
            ]);
    }
}
