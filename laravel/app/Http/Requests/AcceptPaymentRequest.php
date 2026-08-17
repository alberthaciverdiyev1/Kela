<?php

namespace App\Http\Requests;

/** Ödəniş qəbul et — eyni qaydalar həm web, həm API üçün. */
class AcceptPaymentRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'track_id' => ['required', 'integer'],
            'amount' => ['required', 'numeric', 'min:0.01'],
            'note' => ['nullable', 'string', 'max:255'],
        ];
    }
}
