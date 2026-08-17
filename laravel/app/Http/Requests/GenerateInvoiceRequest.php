<?php

namespace App\Http\Requests;

/** Qaimə (ödəniş track) yarat — eyni qaydalar həm web, həm API üçün. */
class GenerateInvoiceRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'student_id' => ['required', 'integer'],
            'workspace_id' => ['required', 'integer'],
            'month' => ['required', 'string', 'regex:/^\d{4}-(0[1-9]|1[0-2])$/'],
            'amount' => ['nullable', 'numeric', 'min:0'],
        ];
    }
}
