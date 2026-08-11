<?php

namespace App\Filament\Resources\Students\Schemas;

use Filament\Infolists\Components\TextEntry;
use Filament\Schemas\Schema;

class StudentInfolist
{
    public static function configure(Schema $schema): Schema
    {
        return $schema
            ->columns(2)
            ->components([
                TextEntry::make('full_name')
                    ->label('Ad Soyad')
                    ->weight('medium'),

                TextEntry::make('email')
                    ->label('E-poçt')
                    ->icon('heroicon-o-envelope'),

                TextEntry::make('studentProfile.city.name')
                    ->label('Şəhər')
                    ->placeholder('-'),

                TextEntry::make('studentProfile.birth_date')
                    ->label('Doğum tarixi')
                    ->date('d M Y')
                    ->placeholder('-'),

                TextEntry::make('status')
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

                TextEntry::make('created_at')
                    ->label('Qeydiyyat')
                    ->dateTime('d M Y H:i')
                    ->placeholder('-'),
            ]);
    }
}
