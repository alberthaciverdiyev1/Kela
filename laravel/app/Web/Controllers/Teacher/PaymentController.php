<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Payment\PaymentService;
use App\Application\Workspace\WorkspaceService;
use App\Http\Requests\AcceptPaymentRequest;
use App\Http\Requests\GenerateInvoiceRequest;
use App\Http\Requests\UpdateTrackRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\RedirectResponse;
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

        // Müddəti ötmüş qaimələri OVERDUE edir.
        $this->payments->markOverdueForTeacher($teacherId);

        $myWorkspaces = $this->workspaces->listForUser($teacherId);
        $selectedWorkspace = $workspaceId ? $this->workspaces->find($workspaceId) : null;
        $tracks = $this->payments->getMonthlySheet($teacherId, $workspaceId, $month);
        $students = $selectedWorkspace ? $this->workspaces->studentList($teacherId, $workspaceId) : [];

        // Hər şagird+sinif üçün növbəti ödəniş tarixləri (bir sorğuda).
        $upcomingDue = $this->payments->upcomingUnpaidDueDates($teacherId);

        // Başlama tarixi (pivot) üçün workspace-lərin şagirdlərini yüklə.
        $tracks->loadMissing(['workspace.students' => fn ($q) => $q->withTrashed()]);

        foreach ($tracks as $track) {
            $pivot = $track->workspace?->students?->firstWhere('id', $track->student_id)?->pivot;

            $startDate = $pivot?->start_date;
            $track->setAttribute('start_date', $startDate);

            // Növbəti ödəniş tarixi:
            // 1) Ödənilməmiş gələcək qaimə varsa → onun due_date-i.
            // 2) Şagird hələ qoşulmayıbsa (gələcək start_date) → ilk ayın sonu.
            // 3) Əks halda → gələn ayın sonu (aylıq abunə proyeksiyası).
            $nextDue = $upcomingDue[$track->student_id.'_'.$track->workspace_id] ?? null;
            if ($nextDue === null && $startDate !== null && \Carbon\Carbon::parse($startDate)->startOfMonth()->gt(now())) {
                $nextDue = \Carbon\Carbon::parse($startDate)->endOfMonth()->toDateString();
            }
            $track->setAttribute('next_due_date', $nextDue ?? now()->addMonth()->endOfMonth()->toDateString());
        }

        // Tarixçə modalı üçün track_id → [tarix, məbləğ, qeyd] xəritəsi.
        $trackTxns = $tracks->mapWithKeys(fn ($t) => [
            (int) $t->id => $t->transactions->map(fn ($x) => [
                'date' => $x->paid_at?->format('d.m.Y H:i') ?? $x->created_at?->format('d.m.Y H:i'),
                'amount' => number_format((float) $x->amount, 2),
                'note' => $x->note ?? '',
            ])->all(),
        ])->all();

        return view('teacher.payments.index', [
            'workspaces' => $myWorkspaces,
            'selectedWorkspaceId' => $workspaceId,
            'selectedWorkspace' => $selectedWorkspace,
            'month' => $month,
            'tracks' => $tracks,
            'students' => $students,
            'trackTxns' => $trackTxns,
        ]);
    }

    public function generate(GenerateInvoiceRequest $request): RedirectResponse
    {
        $data = $request->validated();

        try {
            $this->payments->generateInvoice(
                (int) auth()->id(),
                (int) $data['student_id'],
                (int) $data['workspace_id'],
                $data['month'],
                isset($data['amount']) ? (float) $data['amount'] : null,
            );

            return back()->with('success', 'Tələbə üçün qaimə yaradıldı.');
        } catch (\Exception $e) {
            return back()->with('error', $e->getMessage());
        }
    }

    public function store(AcceptPaymentRequest $request): JsonResponse|RedirectResponse
    {
        $data = $request->validated();

        try {
            $this->payments->acceptPayment(
                (int) auth()->id(),
                (int) $data['track_id'],
                (float) $data['amount'],
                $data['note'] ?? null,
            );

            if ($request->expectsJson()) {
                return response()->json(['message' => 'Ödəniş qəbul edildi.']);
            }

            return back()->with('success', 'Ödəniş uğurla qəbul edildi.');
        } catch (\Exception $e) {
            if ($request->expectsJson()) {
                return response()->json(['error' => $e->getMessage()], 422);
            }
            return back()->with('error', $e->getMessage());
        }
    }

    public function updateTrack(UpdateTrackRequest $request, int $trackId): RedirectResponse
    {
        $data = $request->validated();

        try {
            $this->payments->updateTotalAmount(
                (int) auth()->id(),
                $trackId,
                (float) $data['total_amount'],
            );

            return back()->with('success', 'Məbləğ uğurla yeniləndi.');
        } catch (\Exception $e) {
            return back()->with('error', $e->getMessage());
        }
    }
}
