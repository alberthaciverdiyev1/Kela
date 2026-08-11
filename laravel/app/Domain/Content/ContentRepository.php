<?php

namespace App\Domain\Content;

/**
 * Content (kitabxana elementi) məlumat girişi üçün kontrakt.
 */
interface ContentRepository
{
    public function create(array $attributes): Content;

    public function update(Content $content, array $attributes): Content;

    public function delete(Content $content): bool;

    public function find(int $id): ?Content;

    /** Teacher-in hər Content tipi üzrə sayı: [type => count]. */
    public function countByType(int $teacherId): array;

    /** Teacher-in bütün məzmunları (id, title). */
    public function allForTeacher(int $teacherId): array;
}
