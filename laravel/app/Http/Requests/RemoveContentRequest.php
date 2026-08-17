<?php

namespace App\Http\Requests;

/** Content-i workspace-dən kütüphanəyə geri göndər. */
class RemoveContentRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'content_id' => ['required', 'integer'],
        ];
    }
}
