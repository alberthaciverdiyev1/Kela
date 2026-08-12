<?php

namespace App\Api\Controllers;

use App\Application\Attendance\AttendanceService;
use App\Application\Workspace\WorkspaceService;
use App\Domain\Workspace\Workspace;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class AttendanceController
{
    public function __construct(
        private readonly AttendanceService $attendances,
        private readonly WorkspaceService $workspaces,
    ) {
    }

    /** Müəyyən tarix üçün davam cədvəli (şagirdlər + statuslar). */
    public function show(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace), $request);
        $date = $request->string('date')->toString() ?: now()->toDateString();
        $date = $this->validDateOrAbort($date);

        return response()->json([
            'data' => $this->attendances->sheet((int) $request->user()->id, $workspace, $date),
        ]);
    }

    /** Davam qeydlərini kütləvi yazır. */
    public function store(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace), $request);

        $data = $request->validate([
            'date' => ['required', 'string'],
            'statuses' => ['required', 'array'],
            'statuses.*' => ['integer', 'min:0', 'max:4'],
        ]);

        $date = $this->validDateOrAbort($data['date']);

        $this->attendances->save(
            (int) $request->user()->id,
            $workspace,
            $date,
            $data['statuses'],
            $request->string('note')->toString() ?: null,
        );

        return response()->json(['message' => 'Davam qeydləri saxlanıldı.']);
    }

    /** Müəyyən ay üçün davam cədvəli (şagirdlər + günlər). */
    public function month(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace), $request);
        $month = $request->string('month')->toString() ?: now()->format('Y-m');
        $month = $this->validMonthOrAbort($month);

        return response()->json([
            'data' => $this->attendances->monthSheet((int) $request->user()->id, $workspace, $month),
        ]);
    }

    /** Aylıq davam qeydlərini kütləvi yazır. */
    public function saveMonth(Request $request, int $workspace): JsonResponse
    {
        $this->authorizeAccess($this->workspaces->find($workspace), $request);

        $data = $request->validate([
            'month' => ['required', 'string'],
            'days' => ['required', 'array'],
            'days.*.*' => ['integer', 'min:0', 'max:4'],
        ]);

        $month = $this->validMonthOrAbort($data['month']);

        try {
            $this->attendances->saveMonth(
                (int) $request->user()->id,
                $workspace,
                $month,
                $data['days'],
                $request->string('note')->toString() ?: null,
            );
        } catch (\InvalidArgumentException $e) {
            abort(422, $e->getMessage());
        }

        return response()->json(['message' => 'Davam qeydləri saxlanıldı.']);
    }

    protected function validDateOrAbort(string $date): string
    {
        try {
            return AttendanceService::validDate($date);
        } catch (\InvalidArgumentException $e) {
            abort(422, $e->getMessage());
        }
    }

    protected function validMonthOrAbort(string $month): string
    {
        try {
            return AttendanceService::validMonth($month);
        } catch (\InvalidArgumentException $e) {
            abort(422, $e->getMessage());
        }
    }

    private function authorizeAccess(?Workspace $workspace, Request $request): void
    {
        if ($workspace === null) {
            abort(404);
        }
        $user = $request->user();
        if ($user->isAdmin()) {
            return;
        }
        if ($workspace->teacher_id !== (int) $user->id) {
            abort(403);
        }
    }
}
