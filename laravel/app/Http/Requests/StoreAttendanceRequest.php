<?php

namespace App\Http\Requests;

/**
 * Tək tarix üçün davam (yoklama) qeydlərini kütləvi yaz.
 * workspace_id routa parametridir — bədənə daxil deyil, buna görə qaydada yoxdur.
 */
class StoreAttendanceRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'date' => ['required', 'string'],
            'statuses' => ['required', 'array'],
            'statuses.*' => ['integer', 'min:0', 'max:4'],
            'note' => ['nullable', 'string'],
        ];
    }
}
