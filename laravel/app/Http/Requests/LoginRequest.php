<?php

namespace App\Http\Requests;

/** Giriş (login) — eyni qaydalar həm web sessiyası, həm API token üçün. */
class LoginRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'email' => ['required', 'email'],
            'password' => ['required', 'string'],
        ];
    }
}
