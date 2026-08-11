@props([
    'hasVideo' => false,
    'streamUrl' => '',
    'thumbUrl' => null,
])

<div class="overflow-hidden rounded-xl border border-base-300">
    @if ($hasVideo)
        <video
            class="block aspect-video w-full bg-black"
            controls
            preload="metadata"
            @if ($thumbUrl) poster="{{ $thumbUrl }}" @endif
            src="{{ $streamUrl }}">
            <p>Brauzeriniz video oynatmanı dəstəkləmir.</p>
        </video>
    @else
        <div class="flex aspect-video w-full flex-col items-center justify-center gap-3 bg-base-200 text-base-content/50">
            <x-icon name="heroicon-o-video-camera" class="size-12" />
            <span class="text-sm">Bu dərs üçün hələ video yüklənməyib</span>
        </div>
    @endif
</div>
