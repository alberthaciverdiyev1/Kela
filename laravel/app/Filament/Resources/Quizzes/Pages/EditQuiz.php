<?php

namespace App\Filament\Resources\Quizzes\Pages;

use App\Application\Question\QuestionService;
use App\Application\Quiz\QuizService;
use App\Filament\Resources\Quizzes\QuizResource;
use Filament\Actions\Action;
use Filament\Forms\Components\Select;
use Filament\Forms\Components\TextInput;
use Filament\Notifications\Notification;
use Filament\Schemas\Components\Grid;
use Filament\Resources\Pages\EditRecord;
use Illuminate\Database\Eloquent\Model;

class EditQuiz extends EditRecord
{
    protected static string $resource = QuizResource::class;

    protected string $view = 'filament.resources.quizzes.pages.edit-quiz';

    /** Quiz alanları QuizService üzərindən güncəllənir. */
    protected function handleRecordUpdate(Model $record, array $data): Model
    {
        return app(QuizService::class)->update((int) $record->getKey(), $data);
    }

    protected function getHeaderActions(): array
    {
        return [
            Action::make('addQuestion')
                ->label('Sual Əlavə Et')
                ->icon('heroicon-o-plus-circle')
                ->color('primary')
                ->form([
                    Grid::make(2)->schema([
                        TextInput::make('text')
                            ->label('Sual')
                            ->required()
                            ->columnSpanFull(),

                        TextInput::make('option_a')->label('A')->required(),
                        TextInput::make('option_b')->label('B')->required(),
                        TextInput::make('option_c')->label('C'),
                        TextInput::make('option_d')->label('D'),
                        TextInput::make('option_e')->label('E'),

                        Select::make('correct_option')
                            ->label('Doğru cavab')
                            ->options([0 => 'A', 1 => 'B', 2 => 'C', 3 => 'D', 4 => 'E'])
                            ->required()
                            ->default(0),
                    ]),
                ])
                ->action(function (array $data): void {
                    try {
                        $question = app(QuestionService::class)->create(auth()->id(), $data);
                        app(QuizService::class)->addQuestion(
                            (int) $this->record->getKey(),
                            $question->id,
                            auth()->id(),
                        );
                        Notification::make()->success()->title('Sual əlavə edildi')->send();
                    } catch (\Throwable $e) {
                        Notification::make()->danger()->title($e->getMessage())->send();
                    }
                }),

            \Filament\Actions\DeleteAction::make()
                ->label('Sil')
                ->action(function (): void {
                    app(QuizService::class)->delete((int) $this->record->getKey());
                    $this->redirect(QuizResource::getUrl('index'));
                }),
        ];
    }

    public function getQuestionsDataProperty(): array
    {
        return app(QuizService::class)->questionList((int) $this->record->getKey());
    }

    public function removeQuestion(int $questionId): void
    {
        try {
            app(QuizService::class)->removeQuestion(
                (int) $this->record->getKey(),
                $questionId,
                auth()->id(),
            );
            Notification::make()->success()->title('Sual çıxarıldı')->send();
        } catch (\Throwable $e) {
            Notification::make()->danger()->title($e->getMessage())->send();
        }
    }

    protected function mutateFormDataBeforeFill(array $data): array
    {
        $record = $this->record;
        if ($record && $record->content) {
            $data['title'] = $record->content->title;
            $data['description'] = $record->content->description;
            $data['is_published'] = $record->content->is_published;
        }

        return $data;
    }
}
