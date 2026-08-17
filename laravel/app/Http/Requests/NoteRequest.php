<?php

namespace App\Http\Requests;

use App\Domain\Note\Enums\NoteColor;

/** Qeyd yaratma / yeniləmə — eyni qaydalar həm web, həm API üçün. */
class NoteRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'title' => ['nullable', 'string', 'max:255'],
            'body' => ['nullable', 'string'],
            'color' => ['nullable', 'string', 'in:'.implode(',', NoteColor::values())],
            'is_pinned' => ['nullable', 'boolean'],
        ];
    }
}
