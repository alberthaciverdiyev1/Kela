<?php

namespace Database\Seeders;

use App\Models\BaseSiteConfiguration;
use Illuminate\Database\Seeder;

class SiteConfigSeeder extends Seeder
{
    public function run(): void
    {
        if (BaseSiteConfiguration::query()->exists()) {
            return;
        }

        BaseSiteConfiguration::create([
            'site_name' => 'Kela',
            'primary_color' => '#2563eb',
            'secondary_color' => '#64748b',
            'success_color' => '#22c55e',
            'warning_color' => '#f59e0b',
            'error_color' => '#ef4444',
            'info_color' => '#3b82f6',
            'nav_mode' => 'navbar',
            'notification_provider' => 'sweetalert',
        ]);
    }
}
