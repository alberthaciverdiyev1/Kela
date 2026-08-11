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
        return view('teacher.dashboard', app(DashboardService::class)->counts());
    }

    public function student()
    {
        return view('student.dashboard');
    }

    public function parent()
    {
        return view('family.dashboard');
    }

    public function blocked()
    {
        return view('common.blocked');
    }
}
