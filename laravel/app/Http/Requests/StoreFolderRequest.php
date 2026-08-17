<?php

namespace App\Http\Requests;

/**
 * Yeni qovluq — bütün qovluq növləri üçün (sual/dərs/quiz/workspace).
 * Eyni qaydalar həm web, həm API üçün. Name DB-də varchar(200).
 */
class StoreFolderRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'name' => ['required', 'string', 'max:200'],
            'parent_id' => ['nullable', 'integer'],
        ];
    }
}
