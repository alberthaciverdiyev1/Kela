<?php

namespace App\Api\Controllers;

use App\Application\Note\NoteService;
use App\Domain\Note\Note;
use App\Http\Requests\NoteRequest;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

/**
 * Şəxsi qeydlər (Google Keep üslubu) — hər doğrulanmış istifadəçi üçün.
 */
class NoteController
{
    public function __construct(private readonly NoteService $notes)
    {
    }

    /** İstifadəçinin qeydləri (sabitlənmişlər əvvəl). */
    public function index(Request $request): JsonResponse
    {
        return response()->json([
            'data' => $this->notes->listForUser((int) $request->user()->id),
        ]);
    }

    /** Silinmiş (çöp qutusundakı) qeydlər. */
    public function trashed(Request $request): JsonResponse
    {
        return response()->json([
            'data' => $this->notes->listForUser((int) $request->user()->id, trashed: true),
        ]);
    }

    /** Yeni qeyd yaradır. */
    public function store(NoteRequest $request): JsonResponse
    {
        $data = $request->validated();
        $note = $this->notes->store((int) $request->user()->id, $data);

        return response()->json(['data' => $note], 201);
    }

    /** Qeydi yeniləyir. */
    public function update(NoteRequest $request, int $note): JsonResponse
    {
        $this->authorizeAccess($this->notes->find($note), $request);
        $data = $request->validated();

        return response()->json([
            'data' => $this->notes->update((int) $request->user()->id, $note, $data),
        ]);
    }

    /** Qeydi çöpə atır. */
    public function destroy(Request $request, int $note): JsonResponse
    {
        $this->authorizeAccess($this->notes->find($note), $request);
        $this->notes->destroy((int) $request->user()->id, $note);

        return response()->json(['message' => 'Qeyd çöp qutusuna atıldı.']);
    }

    /** Silinmiş qeydi bərpa edir. */
    public function restore(Request $request, int $note): JsonResponse
    {
        $this->authorizeAccess($this->notes->find($note, withTrashed: true), $request);

        try {
            $note = $this->notes->restore((int) $request->user()->id, $note);
        } catch (\RuntimeException $e) {
            $this->abortFor($e);
        }

        return response()->json(['data' => $note]);
    }

    private function authorizeAccess(?Note $note, Request $request): void
    {
        if ($note === null) {
            abort(404);
        }
        $user = $request->user();
        if ($user->isAdmin()) {
            return;
        }
        if ((int) $note->user_id !== (int) $user->id) {
            abort(403);
        }
    }

    private function abortFor(\RuntimeException $e): never
    {
        abort($e->getMessage() === 'Qeyd tapılmadı.' ? 404 : 403);
    }
}
