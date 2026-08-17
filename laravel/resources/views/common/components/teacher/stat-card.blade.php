@props([
    'label' => null,
    'value' => null,
    'icon' => 'chart-bar',
    'accent' => 'bg-primary',
    'href' => null,
])
<div class="group rounded-xl border border-base-300/80 bg-base-100 p-5 transition-colors duration-200 hover:border-primary/25">
    <div class="flex items-center gap-4">
        <span class="flex size-11 shrink-0 items-center justify-center rounded-lg {{ $accent }} text-white">
            <x-icon name="heroicon-o-{{ $icon }}" class="size-5" />
        </span>
        <div>
            <p class="text-sm font-medium text-base-content/60">{{ $label }}</p>
            <p class="mt-0.5 text-2xl font-bold tracking-tight text-base-content">{{ $value }}</p>
        </div>
    </div>
    @if ($href)
        <a href="{{ $href }}" class="mt-4 inline-flex items-center gap-1 text-sm font-medium text-primary hover:underline">
            <span>{{ $slot }}</span>
        </a>
    @endif
</div>
