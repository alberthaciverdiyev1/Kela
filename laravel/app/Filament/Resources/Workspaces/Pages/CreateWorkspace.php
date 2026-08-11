<?php

namespace App\Filament\Resources\Workspaces\Pages;

use App\Application\Workspace\WorkspaceService;
use App\Filament\Resources\Workspaces\WorkspaceResource;
use Filament\Resources\Pages\CreateRecord;
use Illuminate\Database\Eloquent\Model;

class CreateWorkspace extends CreateRecord
{
    protected static string $resource = WorkspaceResource::class;

    /** Filament model birbaşa yaratmaz — WorkspaceService çağırılır. */
    protected function handleRecordCreation(array $data): Model
    {
        return app(WorkspaceService::class)->create(auth()->id(), $data['name']);
    }
}
