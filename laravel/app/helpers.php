<?php

use App\Domain\StudentPaymentTrack\StudentPaymentTrack;
use Illuminate\Support\Carbon;
use Illuminate\Support\Str;

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

if (! function_exists('full_name')) {
    /**
     * Ad + soyadı bir sətirə yığır:
     *   full_name('Ali', 'Məmmədov')  →  "Ali Məmmədov"
     */
    function full_name(?string $first, ?string $last = null): string
    {
        return trim((string) $first.' '.(string) $last);
    }
}

if (! function_exists('student_status')) {
    /**
     * Şagird statusu → [etiket, badge rəngi]:
     *   student_status(1)  →  ['Aktiv', 'green']
     */
    function student_status(int $status): array
    {
        return match ($status) {
            1 => ['Aktiv', 'green'],
            2 => ['Deaktiv', 'yellow'],
            3 => ['Dayandırılmış', 'red'],
            default => [(string) $status, 'gray'],
        };
    }
}

if (! function_exists('published_color')) {
    /**
     * Yayım statusuna uyğun badge rəngi:
     *   published_color(true)  →  'green'
     */
    function published_color(bool $published): string
    {
        return $published ? 'green' : 'yellow';
    }
}

if (! function_exists('option_letter')) {
    /**
     * Sual variantının indeksini hərfə çevirir:
     *   option_letter(0)  →  'A',  option_letter(1)  →  'B'
     */
    function option_letter(int $index): string
    {
        return chr(65 + $index);
    }
}

if (! function_exists('student_name')) {
    /**
     * Şagird adı; silinmiş şagirddə model null olsa belə 'Şagird #ID' göstərir:
     *   student_name($track->student, $track->student_id)  →  "Ali Məmmədov" / "Şagird #42"
     */
    function student_name(mixed $student, int|string|null $studentId = null): string
    {
        return $student?->full_name ?? ('Şagird #'.$studentId);
    }
}

if (! function_exists('debt')) {
    /**
     * Qalıq borc (mənfi olmaz):
     *   debt(50.00, 20.00)  →  30.0
     */
    function debt(float|int|string|null $total, float|int|string|null $paid): float
    {
        return max(0, (float) ($total ?? 0) - (float) ($paid ?? 0));
    }
}

if (! function_exists('payment_status')) {
    /**
     * Ödəniş qaiməsi → [etiket, badge rəngi].
     * View-dəki if/elseif zəncirinin əvəzi:
     *   [$label, $color] = payment_status($track);
     */
    function payment_status(object $track): array
    {
        if ($track->total_amount > 0 && $track->status == StudentPaymentTrack::STATUS_PAID) {
            return ['Ödənildi', 'green'];
        }
        if ($track->status == StudentPaymentTrack::STATUS_PARTIAL) {
            return ['Qismən Ödəniş', 'blue'];
        }
        if ($track->status == StudentPaymentTrack::STATUS_OVERDUE && $track->total_amount > 0) {
            return ['Vaxtı keçib', 'red'];
        }
        if ($track->status == StudentPaymentTrack::STATUS_CANCELLED) {
            return ['Ləğv edilib', 'gray'];
        }
        if ($track->total_amount == 0) {
            return ['Gözləyir', 'yellow'];
        }

        return ['Ödənilməyib', 'red'];
    }
}

if (! function_exists('limit')) {
    /**
     * Mətnin ilk $length simvolunu qaytarır (HTML-dən təmizləyib):
     *   limit('<p>Uzun mətn</p>', 8)  →  "Uzun mə..."
     */
    function limit(string $text, int $length = 50): string
    {
        return Str::limit(strip_tags($text), $length);
    }
}
