<?php

namespace App\Api\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class LessonResource extends JsonResource
{
    /** @param \App\Domain\Lesson\Lesson $this */
    public function toArray(Request $request): array
    {
        return [
            'content_id' => (int) $this->content_id,
            'folder_id' => $this->folder_id ? (int) $this->folder_id : null,
            'title' => $this->content?->title ?? '',
            'description' => $this->content?->description,
            'is_published' => (bool) $this->is_published,
            'has_video' => (bool) $this->has_video,
            'video_path' => $this->video_path,
            'thumbnail_path' => $this->thumbnail_path,
            'duration_seconds' => (int) $this->duration_seconds,
            'duration_label' => $this->duration_label,
            'order_index' => (int) $this->order_index,
            'created_at' => $this->created_at?->toIso8601String(),
        ];
    }
}
