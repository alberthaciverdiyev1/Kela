<?php

namespace App\Domain\Course;

/**
 * Kurs (course) kavramı — dərslər qrupu.
 *
 * Hələlik verilənlər bazasında ayrıca "courses" cədvəli yoxdur
 * (.NET tərəfində də müvafiq varlıq mövcud deyil). Bu entity və
 * CourseRepository kontraktı gələcəkdə kurs/dərs qruplaşdırması
 * üçün hazırlanmışdır.
 */
class Course
{
    public function __construct(
        public readonly ?int $id,
        public readonly string $title,
        public readonly ?string $description,
    ) {
    }
}
