<?php

namespace App\Http\Requests;

/** Quiz-i qovluğa daşı (null → kök). */
class MoveQuizRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'content_id' => ['required', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ];
    }
}
