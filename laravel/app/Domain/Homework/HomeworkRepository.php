<?php

namespace App\Domain\Homework;

use Illuminate\Contracts\Pagination\LengthAwarePaginator;

/**
 * Ev tapşırığı məlumat girişi üçün kontrakt.
 */
interface HomeworkRepository
{
    public function create(int $teacherId, array $data): Homework;

    /** Cədvəl üçün axtarış + səhifələnmiş ev tapşırığı siyahısı. */
    public function paginateForUser(int $actingUserId, bool $isAdmin, ?string $search = null, int $perPage = 15): LengthAwarePaginator;

    public function find(int $id): ?Homework;

    public function update(Homework $homework, array $data): Homework;

    public function delete(Homework $homework): bool;

    /** Ev tapşırığının sualları (position sırası ilə, HomeworkQuestion[]). */
    public function questions(Homework $homework): array;

    /** Köhnə sualları silir və verilən siyahını (position sırası ilə) yenidən yazır. */
    public function replaceQuestions(Homework $homework, array $questions): void;
}
