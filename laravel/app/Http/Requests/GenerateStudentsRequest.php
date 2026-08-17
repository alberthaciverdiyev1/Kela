<?php

namespace App\Http\Requests;

/**
 * Toplu şagird generasiyası üçün sorğu.
 * Yalnız say tələb olunur — ad, e-poçt, şifrə avtomatik generasiya olunur.
 * Həm şagird siyahısı səhifəsində, həm də workspace-ə əlavə edərkən istifadə olunur.
 */
class GenerateStudentsRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'count' => ['required', 'integer', 'min:1', 'max:100'],
        ];
    }
}
