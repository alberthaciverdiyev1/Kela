<?php

namespace App\Filament\Resources\Quizzes\Pages;

use App\Application\Quiz\QuizService;
use App\Filament\Resources\Quizzes\QuizResource;
use Filament\Resources\Pages\CreateRecord;
use Illuminate\Database\Eloquent\Model;

class CreateQuiz extends CreateRecord
{
    protected static string $resource = QuizResource::class;

    /** Filament model birbaşa yaratmaz — QuizService (Content + Quiz) çağırılır. */
    protected function handleRecordCreation(array $data): Model
    {
        return app(QuizService::class)->create(auth()->id(), $data);
    }

    protected function getRedirectUrl(): string
    {
        return $this->getResource()::getUrl('index');
    }
}
