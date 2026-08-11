<?php

namespace App\Filament\Resources\Lessons\Pages;

use App\Filament\Resources\Lessons\LessonResource;
use App\Models\Content;
use App\Services\MediaProcessor;
use Filament\Resources\Pages\CreateRecord;
use Illuminate\Database\Eloquent\Model;

class CreateLesson extends CreateRecord
{
    protected static string $resource = LessonResource::class;

    /**
     * Dərs = Content (type=lesson) + Lesson satırı.
     * Video yüklənibsə süre/thumbnail burada ffprobe/ffmpeg ile çıxarılır.
     */
    protected function handleRecordCreation(array $data): Model
    {
        if (! empty($data['video_path'])) {
            $meta = app(MediaProcessor::class)->processVideo($data['video_path']);
            $data['thumbnail_path'] = $meta['thumbnail_path'];
            $data['duration_seconds'] = $meta['duration_seconds'];
        }

        $user = auth()->user();

        $content = Content::create([
            'teacher_id' => $user->id,
            'title' => $data['title'] ?? '',
            'description' => $data['description'] ?? null,
            'type' => Content::TYPE_LESSON,
            'is_published' => (bool) ($data['is_published'] ?? false),
        ]);

        $lesson = $this->getModel()::create([
            'content_id' => $content->id,
            'teacher_id' => $user->id,
            'video_path' => $data['video_path'] ?? null,
            'thumbnail_path' => $data['thumbnail_path'] ?? null,
            'duration_seconds' => (int) ($data['duration_seconds'] ?? 0),
            'is_published' => (bool) ($data['is_published'] ?? false),
            'order_index' => (int) ($data['order_index'] ?? 0),
        ]);

        return $lesson->load('content');
    }
}
