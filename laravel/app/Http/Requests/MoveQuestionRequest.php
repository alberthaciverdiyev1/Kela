<?php

namespace App\Http\Requests;

/** Sualı qovluğa daşı (null → kök). */
class MoveQuestionRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'question_id' => ['required', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ];
    }
}
