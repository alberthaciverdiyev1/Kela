<?php

namespace App\Filament\Resources\Students\Pages;

use App\Application\Student\StudentService;
use App\Filament\Resources\Students\StudentResource;
use Filament\Actions\DeleteAction;
use Filament\Actions\ViewAction;
use Filament\Resources\Pages\EditRecord;
use Illuminate\Database\Eloquent\Model;

class EditStudent extends EditRecord
{
    protected static string $resource = StudentResource::class;

    protected function getHeaderActions(): array
    {
        return [
            ViewAction::make(),
            DeleteAction::make(),
        ];
    }

    /**
     * Forma profil alanlarını (şəhər/doğum) doldur — User-də yox, StudentProfile-də.
     */
    protected function mutateFormDataBeforeFill(array $data): array
    {
        $profile = $this->getRecord()->studentProfile;
        $data['city_id'] = $profile?->city_id;
        $data['birth_date'] = $profile?->birth_date?->format('Y-m-d');

        return $data;
    }

    /**
     * Filament birbaşa model istifadə etməz — StudentService çağırılır.
     */
    protected function handleRecordUpdate(Model $record, array $data): Model
    {
        return app(StudentService::class)->update($record->getKey(), $data);
    }
}
