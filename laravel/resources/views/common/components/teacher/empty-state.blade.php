@props([
    'icon' => null,
    'title' => 'Heç nə tapılmadı',
    'description' => null,
])
<div class="flex flex-col items-center justify-center gap-2 px-6 py-16 text-center">
    @if ($icon)
        <x-icon name="heroicon-o-{{ $icon }}" class="size-10 text-base-content/30" />
    @endif
    <p class="text-sm font-semibold text-base-content/80">{{ $title }}</p>
    @if ($description)
        <p class="max-w-sm text-xs text-base-content/50">{{ $description }}</p>
    @endif
</div>
