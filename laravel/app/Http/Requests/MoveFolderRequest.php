<?php

namespace App\Http\Requests;

/** Qovluğu başqa qovluğa daşı (null → kök) — bütün qovluq növləri üçün. */
class MoveFolderRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'parent_id' => ['nullable', 'integer'],
        ];
    }
}
