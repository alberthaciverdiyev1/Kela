<?php

namespace App\Application\Student;

use Illuminate\Support\Facades\Cache;

/**
 * Toplu şagird generasiyasının nəticəsini (Excel məlumatları) cache-də saxlayır.
 * Queue job arxa planda işlədiyi üçün nəticə teacher bazasında saxlanılır:
 *  - "student_export:{teacherId}"        → yaradılan şagird cərgələri (Excel üçün)
 *  - "student_export_running:{teacherId}" → generasiya hələ davam edirmi
 *  - "student_export_failed:{teacherId}"  → generasiya uğursuz oldu mu
 */
class StudentExport
{
    public const TTL = 3600; // 60 dəqiqə

    public static function key(int $teacherId): string
    {
        return "student_export:{$teacherId}";
    }

    public static function runningKey(int $teacherId): string
    {
        return "student_export_running:{$teacherId}";
    }

    public static function failedKey(int $teacherId): string
    {
        return "student_export_failed:{$teacherId}";
    }

    public static function store(int $teacherId, array $rows): void
    {
        Cache::put(self::key($teacherId), $rows, self::TTL);
    }

    /** @return array<int, array{first_name: string, last_name: string, email: string, password: string}> */
    public static function rows(int $teacherId): array
    {
        return (array) Cache::get(self::key($teacherId), []);
    }

    public static function count(int $teacherId): int
    {
        return count(self::rows($teacherId));
    }

    public static function isRunning(int $teacherId): bool
    {
        return (bool) Cache::get(self::runningKey($teacherId), false);
    }

    public static function hasFailed(int $teacherId): bool
    {
        return (bool) Cache::get(self::failedKey($teacherId), false);
    }

    public static function markRunning(int $teacherId, bool $running = true): void
    {
        Cache::put(self::runningKey($teacherId), $running, self::TTL);
    }

    public static function markFailed(int $teacherId): void
    {
        Cache::put(self::failedKey($teacherId), true, self::TTL);
    }

    public static function clear(int $teacherId): void
    {
        Cache::forget(self::key($teacherId));
        Cache::forget(self::runningKey($teacherId));
        Cache::forget(self::failedKey($teacherId));
    }
}
