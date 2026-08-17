<?php

namespace App\Http\Requests;

/** Content-i workspace qovluğuna daşı (null → workspace kökü). */
class MoveContentRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'content_id' => ['required', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'folder_id' => ['nullable', 'integer'],
        ];
    }
}
