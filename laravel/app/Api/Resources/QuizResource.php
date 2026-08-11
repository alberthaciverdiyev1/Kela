<?php

namespace App\Api\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class QuizResource extends JsonResource
{
    /** @param \App\Domain\Quiz\Quiz $this */
    public function toArray(Request $request): array
    {
        return [
            'content_id' => (int) $this->content_id,
            'title' => $this->content?->title ?? '',
            'description' => $this->content?->description,
            'is_published' => (bool) $this->is_published,
            'questions_count' => (int) ($this->questions_count ?? $this->questions()->count()),
            'created_at' => $this->created_at?->toIso8601String(),
        ];
    }
}
