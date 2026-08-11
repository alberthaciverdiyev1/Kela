<?php

namespace App\Filament\Resources\Quizzes\Schemas;

use Filament\Forms\Components\Textarea;
use Filament\Forms\Components\TextInput;
use Filament\Forms\Components\Toggle;
use Filament\Schemas\Schema;

class QuizForm
{
    public static function configure(Schema $schema): Schema
    {
        return $schema
            ->columns(2)
            ->components([
                TextInput::make('title')
                    ->label('Başlıq')
                    ->required()
                    ->maxLength(200)
                    ->columnSpanFull(),

                Textarea::make('description')
                    ->label('Təsvir')
                    ->rows(3)
                    ->columnSpanFull(),

                Toggle::make('is_published')
                    ->label('Yayımlandı')
                    ->default(false),
            ]);
    }
}
