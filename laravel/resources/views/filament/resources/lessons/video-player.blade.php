@php
    /** @var \App\Models\Lesson $lesson */
    $streamUrl = route('lesson.video.stream', $lesson->content_id);
    $thumbUrl = $lesson->thumbnail_path
        ? route('lesson.thumbnail', $lesson->content_id)
        : null;
@endphp

<div class="overflow-hidden rounded-xl border border-gray-200 dark:border-gray-700">
    @if ($lesson->has_video)
        <video
            class="block aspect-video w-full bg-black"
            controls
            preload="metadata"
            poster="{{ $thumbUrl }}"
            src="{{ $streamUrl }}">
            <p>Brauzeriniz video oynatmanı dəstəkləmir.</p>
        </video>
    @else
        <div class="flex aspect-video w-full flex-col items-center justify-center gap-3 bg-gray-100 text-gray-400 dark:bg-gray-800 dark:text-gray-500">
            <x-icon name="heroicons/o-video-camera" class="h-12 w-12" />
            <span class="text-sm">Bu dərs üçün hələ video yüklənməyib</span>
        </div>
    @endif
</div>
