<?php

namespace App\Filament\Resources\Students\Schemas;

use App\Application\City\CityService;
use App\Domain\User\User;
use Filament\Forms\Components\DatePicker;
use Filament\Forms\Components\Select;
use Filament\Forms\Components\TextInput;
use Filament\Schemas\Schema;

class StudentForm
{
    public static function configure(Schema $schema): Schema
    {
        return $schema
            ->columns(2)
            ->components([
                TextInput::make('first_name')
                    ->label('Ad')
                    ->required()
                    ->maxLength(255),

                TextInput::make('last_name')
                    ->label('Soyad')
                    ->maxLength(255),

                TextInput::make('email')
                    ->label('E-poçt')
                    ->email()
                    ->required()
                    ->unique('users', 'email', ignoreRecord: true)
                    ->maxLength(255),

                TextInput::make('password')
                    ->label('Şifrə')
                    ->password()
                    ->revealable()
                    ->dehydrated(fn (?string $state): bool => filled($state))
                    ->required(fn (string $operation): bool => $operation === 'create')
                    ->maxLength(255),

                Select::make('city_id')
                    ->label('Şəhər')
                    ->options(fn (): array => app(CityService::class)->options())
                    ->searchable(),

                DatePicker::make('birth_date')
                    ->label('Doğum tarixi')
                    ->maxDate(today()),

                Select::make('status')
                    ->label('Status')
                    ->options([
                        User::STATUS_ACTIVE => 'Aktiv',
                        User::STATUS_INACTIVE => 'Deaktiv',
                        User::STATUS_SUSPENDED => 'Dayandırılmış',
                    ])
                    ->default(User::STATUS_ACTIVE)
                    ->required(),
            ]);
    }
}
