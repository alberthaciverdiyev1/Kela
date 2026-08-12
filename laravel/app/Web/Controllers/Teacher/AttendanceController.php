<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Attendance\AttendanceService;
use App\Application\Workspace\WorkspaceService;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Davam (yoklama) səhifəsi — klasik aylıq cədvəl.
 * Müəllim workspace seçir, ayın günlərində şagirdlərin iştirakını qeyd edir.
 */
class AttendanceController
{
    public function __construct(
        private readonly WorkspaceService $workspaces,
        private readonly AttendanceService $attendances,
    ) {
    }

    public function index(Request $request): View
    {
        $workspaceId = (int) $request->integer('workspace') ?: null;
        $month = (string) $request->string('month');
        try {
            $month = $month !== '' ? AttendanceService::validMonth($month) : now()->format('Y-m');
        } catch (\InvalidArgumentException) {
            $month = now()->format('Y-m');
        }

        return view('teacher.attendance.index', [
            'workspaces' => $this->workspaces->listForUser((int) auth()->id()),
            'selectedWorkspaceId' => $workspaceId,
            'month' => $month,
        ]);
    }
}
