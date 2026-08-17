<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Attendance\AttendanceService;
use App\Application\Workspace\WorkspaceService;
use App\Http\Requests\StoreAttendanceRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Davam (yoklama) səhifəsi — klasik aylıq cədvəl.
 * Müəllim workspace seçir, ayın günlərində şagirdlərin iştirakını qeyd edir.
 * Cədvəl məlumatı frontend JS tərəfindən web controller endpointlərindən alınır.
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

    /** Aylıq davam cədvəli (şagirdlər + günlər) — JS üçün JSON. */
    public function month(Request $request): JsonResponse
    {
        $workspaceId = (int) $request->integer('workspace');
        $month = (string) $request->string('month');

        try {
            $month = $month !== '' ? AttendanceService::validMonth($month) : now()->format('Y-m');
            $data = $this->attendances->monthSheet((int) auth()->id(), $workspaceId, $month);
        } catch (\RuntimeException $e) {
            abort(404, $e->getMessage());
        } catch (\InvalidArgumentException $e) {
            abort(422, $e->getMessage());
        }

        return response()->json(['data' => $data]);
    }

    /** Tək tarix üçün davam qeydlərini kütləvi yazır — JS üçün JSON. */
    public function store(StoreAttendanceRequest $request): JsonResponse
    {
        $data = $request->validated();

        try {
            $date = AttendanceService::validDate($data['date']);
            $this->attendances->save(
                (int) auth()->id(),
                (int) $request->integer('workspace_id'),
                $date,
                $data['statuses'],
                $request->string('note')->toString() ?: null,
            );
        } catch (\RuntimeException $e) {
            abort(404, $e->getMessage());
        } catch (\InvalidArgumentException $e) {
            abort(422, $e->getMessage());
        }

        return response()->json(['message' => 'Davam qeydləri saxlanıldı.']);
    }
}
