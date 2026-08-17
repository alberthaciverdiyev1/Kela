<?php

namespace App\Http\Requests;

use Illuminate\Foundation\Http\FormRequest;

/**
 * Bütün sorğu (request) siniflərinin əsası.
 *
 * Doğrulama qaydaları TƏK YERDƏ — bu FormRequest siniflərində — saxlanılır.
 * Həm web controller, həm də gələcəkdə yazılacaq API controller eyni sinifi
 * tip-hint edir; beləcə qaydalar iki dəfə yazılmır.
 *
 * authorize() hər yerdə true — icazə yoxlaması servis/controller səviyyəsində
 * aparılır (müəllim sahibliyi, admin əməliyyatları və s.).
 */
abstract class BaseRequest extends FormRequest
{
    public function authorize(): bool
    {
        return true;
    }
}
