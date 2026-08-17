@props([
    'subtitle' => null,
    'icon' => null,
])
<div class="flex flex-wrap items-end justify-between gap-4">
    <div class="flex items-start gap-4">
        @if ($icon)
            <span class="hidden size-12 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br from-primary/15 to-secondary/15 text-primary shadow-sm sm:flex">
                <x-icon name="heroicon-o-{{ $icon }}" class="size-6" />
            </span>
        @endif
        <div>
            <div class="flex items-center gap-3">
                <span class="h-8 w-1.5 rounded-full bg-gradient-to-b from-primary to-secondary"></span>
                <h1 class="text-2xl font-bold tracking-tight text-base-content sm:text-3xl">{{ $slot }}</h1>
            </div>
            @if ($subtitle)
                <p class="mt-2 text-sm text-base-content/60">{{ $subtitle }}</p>
            @endif
        </div>
    </div>
    @isset($actions)
        <div class="flex shrink-0 flex-wrap items-center gap-2">{{ $actions }}</div>
    @endisset
</div>
