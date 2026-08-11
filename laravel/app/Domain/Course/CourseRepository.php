<?php

namespace App\Domain\Course;

use Illuminate\Support\Collection;

/**
 * Kurs repository kontraktı — hələlik yer tutucu.
 * Cədvəl əlavə edildikdə Infrastructure-da Eloquent implementasiyası yazılacaq.
 */
interface CourseRepository
{
    public function find(int $id): ?Course;

    /** @return Collection<int, Course> */
    public function all(): Collection;
}
