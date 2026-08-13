<?php

namespace App\Infrastructure\Persistence\Repositories;

use App\Domain\Note\Note;
use App\Domain\Note\NoteRepository;
use Illuminate\Support\Collection;

class EloquentNoteRepository implements NoteRepository
{
    public function forUser(int $userId, bool $trashed = false): Collection
    {
        return Note::query()
            ->where('user_id', $userId)
            ->when($trashed, fn ($q) => $q->onlyTrashed(), fn ($q) => $q->withoutTrashed())
            ->orderByDesc('is_pinned')
            ->orderByDesc('updated_at')
            ->get();
    }

    public function find(int $noteId, bool $withTrashed = false): ?Note
    {
        return Note::query()
            ->when($withTrashed, fn ($q) => $q->withTrashed())
            ->find($noteId);
    }

    public function store(int $userId, array $data): Note
    {
        return Note::query()->create(array_merge($data, ['user_id' => $userId]));
    }

    public function update(Note $note, array $data): Note
    {
        $note->update($data);

        return $note->fresh();
    }

    public function delete(Note $note): void
    {
        $note->delete();
    }

    public function restore(Note $note): Note
    {
        $note->restore();

        return $note->fresh();
    }
}
