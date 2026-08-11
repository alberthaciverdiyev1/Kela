<?php

namespace App\Application\Course;

use App\Domain\Course\Course;
use App\Domain\Course\CourseRepository;
use Illuminate\Support\Collection;

/**
 * Kurslarla bağlı tətbiq səviyyəli əməliyyatlar.
 *
 * Hələlik yer tutucu: "courses" cədvəli yoxdur. Gələcəkdə dərsləri
 * kurs altında qruplaşdırmaq üçün bu servis genişləndiriləcək.
 */
class CourseService
{
    public function __construct(private readonly CourseRepository $courses)
    {
    }

    public function find(int $id): ?Course
    {
        return $this->courses->find($id);
    }

    /** @return Collection<int, Course> */
    public function list(): Collection
    {
        return $this->courses->all();
    }
}
