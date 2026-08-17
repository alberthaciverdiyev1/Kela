<?php

namespace App\Http\Requests;

/** Workspace yaratma / yeniləmə — eyni qaydalar həm web, həm API üçün. */
class WorkspaceRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'name' => ['required', 'string', 'max:200'],
            'monthly_price' => ['nullable', 'numeric', 'min:0'],
        ];
    }
}
