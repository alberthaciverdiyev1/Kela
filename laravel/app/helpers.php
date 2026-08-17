<?php

use Illuminate\Support\Carbon;

if (! function_exists('dash')) {
    /**
     * Boş dəyəri '—' (em-dash) ilə əvəzləyir.
     * Şablonlarda `?? '—'` yazmağa ehtiyac qalmır:
     *   {{ $student['city'] ?? '—' }}  →  {{ dash($student['city']) }}
     * Null, boş string, boş massiv → '—'. 0 və '0' qorunur.
     */
    function dash(mixed $value, string $fallback = '—'): string
    {
        if (is_array($value)) {
            return $value === [] ? $fallback : (string) json_encode($value);
        }

        return blank($value) ? $fallback : (string) $value;
    }
}

if (! function_exists('money')) {
    /**
     * Pul dəyərini AZN formatında göstərir:
     *   money(50.5)  →  "50.50 AZN"
     *   money(null)  →  "0.00 AZN"
     */
    function money(float|int|string|null $amount): string
    {
        return number_format((float) ($amount ?? 0), 2).' AZN';
    }
}

if (! function_exists('fmt_date')) {
    /**
     * Tarixi 'd.m.Y' (və ya verilən formatda) göstərir; boş → '—'.
     * Carbon instance, string və ya null qəbul edir:
     *   fmt_date($student['birth_date'])        →  "15.05.2010"
     *   fmt_date($track->due_date, 'H:i')       →  "14:30"
     */
    function fmt_date(mixed $date, string $format = 'd.m.Y'): string
    {
        if (blank($date)) {
            return '—';
        }

        return $date instanceof DateTimeInterface
            ? $date->format($format)
            : Carbon::parse($date)->format($format);
    }
}

if (! function_exists('initials')) {
    /**
     * Ad/soyadın baş hərflərini böyük formatda qaytarır (avatar üçün):
     *   initials('Ali', 'Məmmədov')  →  "AM"
     * Hər ikisi boşdursa → "—".
     */
    function initials(?string $first, ?string $last = null): string
    {
        $out = mb_substr((string) $first, 0, 1).mb_substr((string) $last, 0, 1);

        return $out !== '' ? mb_strtoupper($out) : '—';
    }
}
