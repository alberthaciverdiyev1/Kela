@props([
    'title' => null,
    'description' => null,
    'icon' => 'plus',
    'accent' => 'from-primary to-secondary',
    'href' => null,
])
<a href="{{ $href }}"
   class="group relative flex flex-col overflow-hidden rounded-2xl border border-base-300/80 bg-base-100 p-5 shadow-sm shadow-base-300/20 transition-all duration-200 hover:-translate-y-0.5 hover:border-primary/30 hover:shadow-md">
    <span class="flex size-11 items-center justify-center rounded-xl bg-gradient-to-br {{ $accent }} text-white shadow-md shadow-primary/20 transition-transform duration-200 group-hover:scale-105">
        <x-icon name="heroicon-o-{{ $icon }}" class="size-6" />
    </span>
    <h3 class="mt-4 font-semibold text-base-content">{{ $title }}</h3>
    @if ($description)
        <p class="mt-1 text-sm text-base-content/60">{{ $description }}</p>
    @endif
    <span class="mt-3 inline-flex items-center gap-1 text-sm font-medium text-primary">
        <span>Keç</span>
        <x-icon name="heroicon-o-arrow-right" class="size-3.5 transition-transform group-hover:translate-x-0.5" />
    </span>
</a>
