<?php

namespace App\Api\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class QuestionResource extends JsonResource
{
    /** @param \App\Domain\Question\Question $this */
    public function toArray(Request $request): array
    {
        return [
            'id' => (int) $this->id,
            'text' => $this->text,
            'options' => $this->options(),
            'correct_option' => (int) $this->correct_option,
            'created_at' => $this->created_at?->toIso8601String(),
        ];
    }
}
