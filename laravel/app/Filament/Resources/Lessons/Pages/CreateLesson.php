<?php

namespace App\Filament\Resources\Lessons\Pages;

use App\Application\Lesson\LessonService;
use App\Filament\Resources\Lessons\LessonResource;
use Filament\Resources\Pages\CreateRecord;
use Illuminate\Database\Eloquent\Model;

class CreateLesson extends CreateRecord
{
    protected static string $resource = LessonResource::class;

    /**
     * Dərs = Content (type=lesson) + Lesson sətri.
     * Filament birbaşa model istifadə etməz — LessonService çağırılır.
     */
    protected function handleRecordCreation(array $data): Model
    {
        return app(LessonService::class)->create(auth()->id(), $data);
    }
}
