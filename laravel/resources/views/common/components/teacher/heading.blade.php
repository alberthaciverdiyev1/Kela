@props([
    'subtitle' => null,
])
<div class="flex items-end justify-between gap-4">
    <div>
        <h1 class="text-xl font-bold tracking-tight text-base-content">{{ $slot }}</h1>
        @if ($subtitle)
            <p class="mt-1 text-sm text-base-content/60">{{ $subtitle }}</p>
        @endif
    </div>
    @isset($actions)
        <div class="flex shrink-0 items-center gap-2">{{ $actions }}</div>
    @endisset
</div>
