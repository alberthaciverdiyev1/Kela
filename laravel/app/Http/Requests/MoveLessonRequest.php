<?php

namespace App\Http\Requests;

/** Dərsi qovluğa daşı (null → kök). */
class MoveLessonRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'content_id' => ['required', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ];
    }
}
