<?php

namespace App\Application\Note;

use App\Domain\Note\Note;
use App\Domain\Note\NoteRepository;
use App\Domain\User\User;

/**
 * Şəxsi qeydlər (Google Keep üslubu).
 *
 * Hər qeyd bir istifadəçiyə aiddir; admin istisna olmaqla başqasının
 * qeydinə toxunmaq olmur.
 */
class NoteService
{
    public function __construct(private readonly NoteRepository $notes)
    {
    }

    /** İstifadəçinin qeydləri (sabitlənmişlər əvvəl). */
    public function listForUser(int $userId, bool $trashed = false): array
    {
        return $this->notes->forUser($userId, $trashed)
            ->map(fn (Note $n) => $this->payload($n))
            ->values()
            ->all();
    }

    /** Qeydi tapır — sahiblik yoxlanmadan (API controlleraları üçün). */
    public function find(int $noteId, bool $withTrashed = false): ?Note
    {
        return $this->notes->find($noteId, $withTrashed);
    }

    /**
     * Yeni qeyd yaradır.
     *
     * @param array{title?: string|null, body?: string|null, color?: string, is_pinned?: bool} $data
     */
    public function store(int $userId, array $data): array
    {
        $note = $this->notes->store($userId, [
            'title' => isset($data['title']) && trim((string) $data['title']) !== '' ? trim((string) $data['title']) : null,
            'body' => isset($data['body']) && trim((string) $data['body']) !== '' ? trim((string) $data['body']) : null,
            'color' => $this->validColor($data['color'] ?? 'default'),
            'is_pinned' => (bool) ($data['is_pinned'] ?? false),
        ]);

        return $this->payload($note);
    }

    /**
     * Qeydi yeniləyir (yalnız verilən sahələr).
     *
     * @param array{title?: string|null, body?: string|null, color?: string, is_pinned?: bool} $data
     */
    public function update(int $actingUserId, int $noteId, array $data): array
    {
        $note = $this->assertOwned($actingUserId, $noteId);

        $changes = [];
        if (array_key_exists('title', $data)) {
            $changes['title'] = trim((string) $data['title']) !== '' ? trim((string) $data['title']) : null;
        }
        if (array_key_exists('body', $data)) {
            $changes['body'] = trim((string) $data['body']) !== '' ? trim((string) $data['body']) : null;
        }
        if (array_key_exists('color', $data)) {
            $changes['color'] = $this->validColor($data['color']);
        }
        if (array_key_exists('is_pinned', $data)) {
            $changes['is_pinned'] = (bool) $data['is_pinned'];
        }

        if ($changes === []) {
            return $this->payload($note);
        }

        return $this->payload($this->notes->update($note, $changes));
    }

    /** Qeydi çöpə atır (soft delete). */
    public function destroy(int $actingUserId, int $noteId): void
    {
        $note = $this->assertOwned($actingUserId, $noteId);
        $this->notes->delete($note);
    }

    /** Silinmiş qeydi bərpa edir. */
    public function restore(int $actingUserId, int $noteId): array
    {
        $note = $this->assertOwned($actingUserId, $noteId, withTrashed: true);
        return $this->payload($this->notes->restore($note));
    }

    /** Rəng açarı mövcud palitrada olmalıdır. */
    public function validColor(string $color): string
    {
        if (! in_array($color, Note::COLORS, true)) {
            throw new \InvalidArgumentException('Keçərsiz qeyd rəngi.');
        }

        return $color;
    }

    protected function assertOwned(int $actingUserId, int $noteId, bool $withTrashed = false): Note
    {
        $note = $this->notes->find($noteId, $withTrashed);
        if ($note === null) {
            throw new \RuntimeException('Qeyd tapılmadı.');
        }

        $user = User::find($actingUserId);
        if ($user?->isAdmin()) {
            return $note;
        }
        if ((int) $note->user_id !== $actingUserId) {
            throw new \RuntimeException('Bu qeydə icazəniz yoxdur.');
        }

        return $note;
    }

    /** API üçün təmiz JSON payload. */
    private function payload(Note $note): array
    {
        return [
            'id' => (int) $note->id,
            'title' => $note->title,
            'body' => $note->body,
            'color' => $note->color,
            'is_pinned' => (bool) $note->is_pinned,
            'updated_at' => $note->updated_at?->toIso8601String(),
            'deleted_at' => $note->deleted_at?->toIso8601String(),
        ];
    }
}
