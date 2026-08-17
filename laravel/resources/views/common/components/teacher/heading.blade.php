@props([
    'subtitle' => null,
    'icon' => null,
])
<div class="flex flex-wrap items-end justify-between gap-4">
    <div class="flex items-start gap-4">
        @if ($icon)
            <span class="hidden size-12 shrink-0 items-center justify-center rounded-xl bg-primary/10 text-primary sm:flex">
                <x-icon name="heroicon-o-{{ $icon }}" class="size-6" />
            </span>
        @endif
        <div>
            <h1 class="text-2xl font-bold tracking-tight text-base-content sm:text-3xl">{{ $slot }}</h1>
            @if ($subtitle)
                <p class="mt-2 text-sm text-base-content/60">{{ $subtitle }}</p>
            @endif
        </div>
    </div>
    @isset($actions)
        <div class="flex shrink-0 flex-wrap items-center gap-2">{{ $actions }}</div>
    @endisset
</div>
