<?php

namespace App\Filament\Resources\Lessons\Pages;

use App\Filament\Resources\Lessons\LessonResource;
use App\Services\MediaProcessor;
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
     * Content başlıq/təsvir/yayım alanlarını günceller; video dəyişdiysə
     * süre/thumbnail yenidən hesablanır.
     */
    protected function handleRecordUpdate(Model $record, array $data): Model
    {
        if (empty($data['video_path'])) {
            $data['thumbnail_path'] = null;
            $data['duration_seconds'] = 0;
        } elseif ($data['video_path'] !== $record->video_path) {
            $meta = app(MediaProcessor::class)->processVideo($data['video_path']);
            $data['thumbnail_path'] = $meta['thumbnail_path'];
            $data['duration_seconds'] = $meta['duration_seconds'];
        }

        if ($record->content) {
            $record->content->update([
                'title' => $data['title'] ?? '',
                'description' => $data['description'] ?? null,
                'is_published' => (bool) ($data['is_published'] ?? false),
            ]);
        }

        $record->update([
            'video_path' => $data['video_path'] ?? null,
            'thumbnail_path' => $data['thumbnail_path'] ?? null,
            'duration_seconds' => (int) ($data['duration_seconds'] ?? 0),
            'is_published' => (bool) ($data['is_published'] ?? false),
            'order_index' => (int) ($data['order_index'] ?? 0),
        ]);

        return $record->load('content');
    }
}
