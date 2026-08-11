<?php

namespace App\Infrastructure\Http\Controllers;

use App\Domain\BaseSiteConfiguration\BaseSiteConfiguration;
use App\Domain\User\User;
use Illuminate\Support\Facades\Auth;

class DashboardController extends Controller
{
    public function admin()
    {
        return view('dashboards.admin', [
            'teachers' => User::role('Teacher')->count(),
            'students' => User::role('Student')->count(),
        ]);
    }

    public function teacher()
    {
        $user = Auth::user();
        return view('dashboards.teacher', [
            'workspaceCount' => $user->workspaces()->count(),
            'studentCount' => $user->workspaces()->withCount('students')->get()->sum('students_count'),
            'contentCount' => $user->contents()->count(),
        ]);
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
