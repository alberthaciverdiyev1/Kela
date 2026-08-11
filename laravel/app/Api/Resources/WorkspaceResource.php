<?php

namespace App\Api\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class WorkspaceResource extends JsonResource
{
    /** @param \App\Domain\Workspace\Workspace $this */
    public function toArray(Request $request): array
    {
        return [
            'id' => (int) $this->id,
            'name' => $this->name,
            'teacher_id' => (int) $this->teacher_id,
            'created_at' => $this->created_at?->toIso8601String(),
        ];
    }
}
