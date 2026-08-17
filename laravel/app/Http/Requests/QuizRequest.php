<?php

namespace App\Http\Requests;

/**
 * Quiz yaratma / yeniləmə — eyni qaydalar həm web, həm API üçün.
 * Title DB-də varchar(200) — hər iki tərəfdə max:200 tətbiq olunur.
 */
class QuizRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'title' => ['required', 'string', 'max:200'],
            'description' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'folder_id' => ['nullable', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'ws_folder_id' => ['nullable', 'integer'],
        ];
    }
}
