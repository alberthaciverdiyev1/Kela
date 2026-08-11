<?php

namespace App\Web\Controllers;

use App\Application\Teacher\DashboardService;
use App\Domain\BaseSiteConfiguration\BaseSiteConfiguration;
use Illuminate\Support\Facades\Auth;

class DashboardController extends Controller
{
    public function teacher()
    {
        // Layered qayda: controller modellərə toxunmur, DashboardService-ə gedir.
        return view('dashboards.teacher', app(DashboardService::class)->counts());
    }

    public function student()
    {
        return view('dashboards.student');
    }

    public function parent()
    {
        return view('dashboards.parent');
    }

    public function blocked()
    {
        return view('dashboards.blocked');
    }
}
