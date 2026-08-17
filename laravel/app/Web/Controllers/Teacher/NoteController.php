<?php

namespace App\Web\Controllers\Teacher;

use App\Application\Note\NoteService;
use App\Http\Requests\NoteRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\View\View;

/**
 * Qeydlər səhifəsi (teacher paneli) — Google Keep üslubu.
 * Qeyd əməliyyatları web controller üzərindən (JSON) aparılır — frontend
 * /api/v1-ə birbaşa toxunmur.
 */
class NoteController
{
    public function __construct(private readonly NoteService $notes)
    {
    }

    public function index(): View
    {
        return view('teacher.notes.index');
    }

    /** İstifadəçinin qeydləri (sabitlənmişlər əvvəl). */
    public function indexJson(): JsonResponse
    {
        return response()->json([
            'data' => $this->notes->listForUser((int) auth()->id()),
        ]);
    }

    /** Silinmiş (çöp qutusundakı) qeydlər. */
    public function trashedJson(): JsonResponse
    {
        return response()->json([
            'data' => $this->notes->listForUser((int) auth()->id(), trashed: true),
        ]);
    }

    /** Yeni qeyd yaradır. */
    public function storeJson(NoteRequest $request): JsonResponse
    {
        $data = $request->validated();
        $note = $this->notes->store((int) auth()->id(), $data);

        return response()->json(['data' => $note], 201);
    }

    /** Qeydi yeniləyir. */
    public function updateJson(NoteRequest $request, int $note): JsonResponse
    {
        $data = $request->validated();

        try {
            $note = $this->notes->update((int) auth()->id(), $note, $data);
        } catch (\RuntimeException $e) {
            $this->abortFor($e);
        }

        return response()->json(['data' => $note]);
    }

    /** Qeydi çöpə atır. */
    public function destroyJson(int $note): JsonResponse
    {
        try {
            $this->notes->destroy((int) auth()->id(), $note);
        } catch (\RuntimeException $e) {
            $this->abortFor($e);
        }

        return response()->json(['message' => 'Qeyd çöp qutusuna atıldı.']);
    }

    /** Silinmiş qeydi bərpa edir. */
    public function restoreJson(int $note): JsonResponse
    {
        try {
            $note = $this->notes->restore((int) auth()->id(), $note);
        } catch (\RuntimeException $e) {
            $this->abortFor($e);
        }

        return response()->json(['data' => $note]);
    }

    protected function abortFor(\RuntimeException $e): never
    {
        abort($e->getMessage() === 'Qeyd tapılmadı.' ? 404 : 403);
    }
}
