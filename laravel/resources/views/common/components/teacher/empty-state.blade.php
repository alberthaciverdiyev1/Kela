@props([
    'icon' => null,
    'title' => 'Heç nə tapılmadı',
    'description' => null,
])
<div class="flex flex-col items-center justify-center gap-4 px-6 py-16 text-center">
    @if ($icon)
        <div class="flex size-14 items-center justify-center rounded-full bg-base-200/50">
            <x-icon name="heroicon-o-{{ $icon }}" class="size-7 text-base-content/40" />
        </div>
    @endif
    <div>
        <h3 class="text-sm font-semibold text-base-content">{{ $title }}</h3>
        @if ($description)
            <p class="mt-1 max-w-sm text-sm text-base-content/60">{{ $description }}</p>
        @endif
    </div>
    @if ($slot->isNotEmpty())
        <div class="mt-2">
            {{ $slot }}
        </div>
    @endif
</div>
