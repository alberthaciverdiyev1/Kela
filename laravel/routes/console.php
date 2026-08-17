<?php

use Illuminate\Foundation\Inspiring;
use Illuminate\Support\Facades\Artisan;

use Illuminate\Support\Facades\Schedule;

Artisan::command('inspire', function () {
    $this->comment(Inspiring::quote());
})->purpose('Display an inspiring quote');

Schedule::command('payments:generate')->monthlyOn(1, '00:00');

// Ödəniş müddəti yaxınlaşan/qalan şagirdlər üçün bildirişlər — hər saat yoxlanır.
Schedule::command('payments:remind')->hourly();
