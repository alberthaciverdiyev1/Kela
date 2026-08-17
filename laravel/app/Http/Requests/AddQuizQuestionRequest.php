<?php

namespace App\Http\Requests;

/** Bankdan sualı quizə bağla. */
class AddQuizQuestionRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'question_id' => ['required', 'integer'],
        ];
    }
}
