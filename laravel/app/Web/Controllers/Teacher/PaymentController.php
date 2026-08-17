<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Payment\PaymentService;
use App\Application\Workspace\WorkspaceService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;
use Illuminate\Routing\Controller;

class PaymentController extends Controller
{
    public function __construct(
        private readonly PaymentService $payments,
        private readonly WorkspaceService $workspaces,
    ) {
    }

    public function index(Request $request): View
    {
        $workspaceId = (int) $request->integer('workspace') ?: null;
        $month = (string) $request->string('month');
        
        if (empty($month)) {
            $month = now()->format('Y-m');
        }

        $teacherId = (int) auth()->id();
        
        $myWorkspaces = $this->workspaces->listForUser($teacherId);

        $selectedWorkspace = $workspaceId ? $this->workspaces->find($workspaceId) : null;

        $tracks = $this->payments->getMonthlySheet($teacherId, $workspaceId, $month);

        return view('teacher.payments.index', [
            'workspaces' => $myWorkspaces,
            'selectedWorkspaceId' => $workspaceId,
            'selectedWorkspace' => $selectedWorkspace,
            'month' => $month,
            'tracks' => $tracks,
        ]);
    }
    
    public function generate(Request $request): \Illuminate\Http\RedirectResponse
    {
        $studentId = (int) $request->integer('student_id');
        $workspaceId = (int) $request->integer('workspace_id');
        $month = $request->string('month')->toString();
        $amount = $request->filled('amount') ? (float) $request->input('amount') : null;
        
        try {
            $this->payments->generateInvoice($studentId, $workspaceId, $month, $amount);
            return back()->with('success', 'Tələbə üçün qaimə yaradıldı.');
        } catch (\Exception $e) {
            return back()->with('error', $e->getMessage());
        }
    }

    public function store(Request $request): JsonResponse|\Illuminate\Http\RedirectResponse
    {
        $request->validate([
            'track_id' => 'required|integer',
            'amount' => 'required|numeric|min:0.01',
            'note' => 'nullable|string',
        ]);

        try {
            $this->payments->acceptPayment(
                (int) $request->input('track_id'),
                (float) $request->input('amount'),
                $request->input('note')
            );
            
            if ($request->wantsJson()) {
                return response()->json(['message' => 'Ödəniş qəbul edildi.']);
            }
            
            return back()->with('success', 'Ödəniş uğurla qəbul edildi.');
        } catch (\Exception $e) {
            if ($request->wantsJson()) {
                return response()->json(['error' => $e->getMessage()], 422);
            }
            return back()->with('error', $e->getMessage());
        }
    }

    public function updateTrack(Request $request, int $trackId): \Illuminate\Http\RedirectResponse
    {
        $request->validate([
            'total_amount' => 'required|numeric|min:0',
        ]);
        
        try {
            $this->payments->updateTotalAmount($trackId, (float) $request->input('total_amount'));
            return back()->with('success', 'Məbləğ uğurla yeniləndi.');
        } catch (\Exception $e) {
            return back()->with('error', $e->getMessage());
        }
    }
}
