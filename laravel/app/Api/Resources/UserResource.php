<?php

namespace App\Api\Resources;

use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\JsonResource;

class UserResource extends JsonResource
{
    /** @param \App\Domain\User\User $this */
    public function toArray(Request $request): array
    {
        return [
            'id' => (int) $this->id,
            'first_name' => $this->first_name,
            'last_name' => $this->last_name,
            'full_name' => $this->full_name,
            'email' => $this->email,
            'roles' => $this->getRoleNames()->values()->all(),
            'status' => (int) $this->status,
            'home_route' => $this->homeRoute(),
        ];
    }
}
