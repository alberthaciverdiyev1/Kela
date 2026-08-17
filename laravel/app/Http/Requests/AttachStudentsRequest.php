<?php

namespace App\Http\Requests;

/** Workspace-ə tələbələri əlavə et. */
class AttachStudentsRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'student_ids' => ['required', 'array'],
            'student_ids.*' => ['integer'],
        ];
    }
}
