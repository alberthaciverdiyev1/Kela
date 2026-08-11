<?php

namespace App\Filament\Resources\Lessons\Pages;

use App\Application\Lesson\LessonService;
use App\Filament\Resources\Lessons\LessonResource;
use Filament\Actions\DeleteAction;
use Filament\Actions\ViewAction;
use Filament\Resources\Pages\EditRecord;
use Illuminate\Database\Eloquent\Model;

class EditLesson extends EditRecord
{
    protected static string $resource = LessonResource::class;

    protected function getHeaderActions(): array
    {
        return [
            ViewAction::make(),
            DeleteAction::make(),
        ];
    }

    /**
     * Filament birbaşa model istifadə etməz — LessonService çağırılır.
     */
    protected function handleRecordUpdate(Model $record, array $data): Model
    {
        return app(LessonService::class)->update($record->getKey(), $data);
    }
}
