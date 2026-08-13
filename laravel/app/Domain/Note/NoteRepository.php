<?php

namespace App\Domain\Note;

use Illuminate\Support\Collection;

/**
 * Şəxsi qeydlər üçün məlumat girişi kontraktı.
 */
interface NoteRepository
{
    /** İstifadəçinin qeydləri (sabitlənmişlər əvvəl). $trashed=true → silinmişlər. */
    public function forUser(int $userId, bool $trashed = false): Collection;

    /** Qeydi tapır (silinmiş daxil edilə bilər). */
    public function find(int $noteId, bool $withTrashed = false): ?Note;

    /** Yeni qeyd yaradır. */
    public function store(int $userId, array $data): Note;

    /** Qeydi yeniləyir. */
    public function update(Note $note, array $data): Note;

    /** Qeydi silir (soft delete). */
    public function delete(Note $note): void;

    /** Silinmiş qeydi bərpa edir. */
    public function restore(Note $note): Note;
}
