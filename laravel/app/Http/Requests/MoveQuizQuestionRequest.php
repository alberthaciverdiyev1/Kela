<?php

namespace App\Http\Requests;

/** Quiz daxilində sualın sırasını dəyiş (yuxarı/aşağı). */
class MoveQuizQuestionRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'direction' => ['required', 'in:up,down'],
        ];
    }
}
