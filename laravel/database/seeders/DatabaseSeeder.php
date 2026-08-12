<?php

namespace Database\Seeders;

use Illuminate\Database\Seeder;

class DatabaseSeeder extends Seeder
{
    public function run(): void
    {
        $this->call([
            RoleSeeder::class,
            CitySeeder::class,
            DemoUserSeeder::class,
            SiteConfigSeeder::class,
            DemoContentSeeder::class,
            DemoLessonContentSeeder::class,
        ]);
    }
}
