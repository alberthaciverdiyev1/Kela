<?php

namespace App\Filament\Resources\Workspaces\Pages;

use App\Application\Workspace\WorkspaceService;
use App\Filament\Resources\Workspaces\WorkspaceResource;
use Filament\Resources\Pages\EditRecord;
use Illuminate\Database\Eloquent\Model;

class EditWorkspace extends EditRecord
{
    protected static string $resource = WorkspaceResource::class;

    protected function handleRecordUpdate(Model $record, array $data): Model
    {
        app(WorkspaceService::class)->rename(auth()->id(), (int) $record->getKey(), $data['name']);

        return $record->refresh();
    }

    protected function getRedirectUrl(): string
    {
        return $this->getResource()::getUrl('index');
    }
}
