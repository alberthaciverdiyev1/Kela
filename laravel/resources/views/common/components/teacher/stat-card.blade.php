@props([
    'label' => null,
    'value' => null,
    'icon' => 'chart-bar',
    'accent' => 'from-primary to-secondary',
    'href' => null,
])
<div class="group relative overflow-hidden rounded-2xl border border-base-300/80 bg-base-100 p-6 shadow-sm shadow-base-300/20 transition-all duration-200 hover:-translate-y-0.5 hover:shadow-md">
    <div aria-hidden="true" class="pointer-events-none absolute -right-8 -top-8 size-28 rounded-full bg-gradient-to-br {{ $accent }} opacity-10 blur-2xl transition-opacity duration-200 group-hover:opacity-20"></div>
    <div class="flex items-center gap-4">
        <span class="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-gradient-to-br {{ $accent }} text-white shadow-md shadow-primary/20">
            <x-icon name="heroicon-o-{{ $icon }}" class="size-6" />
        </span>
        <div>
            <p class="text-sm font-medium text-base-content/60">{{ $label }}</p>
            <p class="mt-0.5 text-2xl font-bold tracking-tight text-base-content">{{ $value }}</p>
        </div>
    </div>
    @if ($href)
        <a href="{{ $href }}" class="mt-4 inline-flex items-center gap-1 text-sm font-medium text-primary hover:underline">
            <span>{{ $slot }}</span>
            <x-icon name="heroicon-o-arrow-right" class="size-3.5 transition-transform group-hover:translate-x-0.5" />
        </a>
    @endif
</div>
