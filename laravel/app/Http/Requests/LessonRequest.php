<?php

namespace App\Http\Requests;

/**
 * Dərs yaratma / yeniləmə — eyni qaydalar həm web, həm API üçün.
 * Web formu `video` faylını, API isə `video_path` sətirini göndərir —
 * hər ikisi nullable-dır, eyni sinif hər ikisini qəbul edir.
 */
class LessonRequest extends BaseRequest
{
    public function rules(): array
    {
        return [
            'title' => ['required', 'string', 'max:255'],
            'description' => ['nullable', 'string'],
            'video' => ['nullable', 'file', 'min:1', 'max:524288', 'mimetypes:video/mp4,video/webm,video/ogg,video/quicktime,video/x-m4v,video/x-matroska,video/x-msvideo,video/mpeg'],
            'video_path' => ['nullable', 'string'],
            'is_published' => ['nullable', 'boolean'],
            'order_index' => ['nullable', 'integer', 'min:0'],
            'folder_id' => ['nullable', 'integer'],
            'workspace_id' => ['nullable', 'integer'],
            'ws_folder_id' => ['nullable', 'integer'],
        ];
    }
}
