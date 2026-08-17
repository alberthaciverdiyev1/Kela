<?php

namespace App\Http\Requests;

/**
 * Ev tapşırığı yaratma / yeniləmə.
 * Sual kompozisiyası questions_json sahəsində ötürülür (editor səhifəsi).
 */
class HomeworkRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'title' => ['required', 'string', 'max:200'],
            'description' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'questions_json' => ['nullable', 'string'],
        ];
    }
}
