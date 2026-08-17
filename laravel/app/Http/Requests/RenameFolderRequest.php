<?php

namespace App\Http\Requests;

/** Qovluq adını dəyiş — bütün qovluq növləri üçün. */
class RenameFolderRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'name' => ['required', 'string', 'max:200'],
        ];
    }
}
