<?php

namespace App\Filament\Resources\Lessons\Schemas;

use Filament\Forms\Components\FileUpload;
use Filament\Forms\Components\Hidden;
use Filament\Forms\Components\Textarea;
use Filament\Forms\Components\TextInput;
use Filament\Forms\Components\Toggle;
use Filament\Schemas\Schema;

class LessonForm
{
    public static function configure(Schema $schema): Schema
    {
        return $schema
            ->columns(3)
            ->components([
                TextInput::make('title')
                    ->label('Başlıq')
                    ->required()
                    ->maxLength(255)
                    ->columnSpanFull(),

                Textarea::make('description')
                    ->label('Təsvir')
                    ->rows(4)
                    ->columnSpanFull(),

                FileUpload::make('video_path')
                    ->label('Video faylı')
                    ->disk('local')
                    ->directory(\App\Services\MediaProcessor::VIDEOS_DIR)
                    ->acceptedFileTypes([
                        'video/mp4', 'video/webm', 'video/ogg',
                        'video/quicktime', 'video/x-m4v',
                        'video/x-matroska', 'video/x-msvideo', 'video/mpeg',
                    ])
                    ->maxSize(\App\Services\MediaProcessor::MAX_VIDEO_MB * 1024)
                    ->helperText('Maksimum 512 MB — MP4, WebM, MKV, AVI, MOV')
                    ->openable()
                    ->downloadable()
                    ->previewable(false)
                    ->columnSpanFull(),

                Toggle::make('is_published')
                    ->label('Yayımlandı')
                    ->default(false),

                TextInput::make('order_index')
                    ->label('Sıra')
                    ->numeric()
                    ->default(0)
                    ->minValue(0),

                TextInput::make('duration_seconds')
                    ->label('Müddət (saniyə)')
                    ->numeric()
                    ->readOnly()
                    ->dehydrated(),

                Hidden::make('thumbnail_path'),
            ]);
    }
}
