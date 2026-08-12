<?php

namespace Database\Seeders;

use App\Domain\City\City;
use Illuminate\Database\Seeder;

class CitySeeder extends Seeder
{
    public function run(): void
    {
        $cities = [
            ['az' => 'Bakı', 'en' => 'Baku', 'ru' => 'Баку', 'tr' => 'Bakü'],
            ['az' => 'Gəncə', 'en' => 'Ganja', 'ru' => 'Гянджа', 'tr' => 'Gence'],
            ['az' => 'Moskva', 'en' => 'Moscow', 'ru' => 'Москва', 'tr' => 'Moskova'],
            ['az' => 'İstanbul', 'en' => 'Istanbul', 'ru' => 'Стамбул', 'tr' => 'İstanbul'],
            ['az' => 'Ankara', 'en' => 'Ankara', 'ru' => 'Анкара', 'tr' => 'Ankara'],
            ['az' => 'London', 'en' => 'London', 'ru' => 'Лондон', 'tr' => 'Londra'],
            ['az' => 'Paris', 'en' => 'Paris', 'ru' => 'Париж', 'tr' => 'Paris'],
            ['az' => 'Berlin', 'en' => 'Berlin', 'ru' => 'Берлин', 'tr' => 'Berlin'],
            ['az' => 'Barselona', 'en' => 'Barcelona', 'ru' => 'Барселона', 'tr' => 'Barselona'],
            ['az' => 'Roma', 'en' => 'Rome', 'ru' => 'Рим', 'tr' => 'Roma'],
        ];

        foreach ($cities as $names) {
            // JSONB axtarış: massiv firstOrCreate-də where-ə verilə bilməz.
            $city = City::where('name_translations->az', $names['az'])->first();
            if (! $city) {
                City::create(['name_translations' => $names]);
            }
        }
    }
}
