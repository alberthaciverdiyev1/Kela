<?php

namespace App\Http\Requests;

/** Qaimə ümumi məbləğini yenilə — eyni qaydalar həm web, həm API üçün. */
class UpdateTrackRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'total_amount' => ['required', 'numeric', 'min:0'],
        ];
    }
}
