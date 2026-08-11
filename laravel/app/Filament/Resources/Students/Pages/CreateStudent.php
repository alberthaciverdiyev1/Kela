<?php

namespace App\Filament\Resources\Students\Pages;

use App\Application\Student\StudentService;
use App\Filament\Resources\Students\StudentResource;
use Filament\Resources\Pages\CreateRecord;
use Illuminate\Database\Eloquent\Model;

class CreateStudent extends CreateRecord
{
    protected static string $resource = StudentResource::class;

    /**
     * Filament birbaşa model istifadə etməz — StudentService çağırılır.
     */
    protected function handleRecordCreation(array $data): Model
    {
        return app(StudentService::class)->create($data);
    }
}
