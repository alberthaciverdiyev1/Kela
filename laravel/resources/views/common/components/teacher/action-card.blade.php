@props([
    'title' => null,
    'description' => null,
    'icon' => 'plus',
    'accent' => 'bg-primary',
    'href' => null,
])
<a href="{{ $href }}"
   class="group flex flex-col rounded-xl border border-base-300/80 bg-base-100 p-5 transition-colors duration-200 hover:border-primary/25">
    <span class="flex size-10 items-center justify-center rounded-lg {{ $accent }} text-white">
        <x-icon name="heroicon-o-{{ $icon }}" class="size-5" />
    </span>
    <h3 class="mt-3 font-semibold text-base-content">{{ $title }}</h3>
    @if ($description)
        <p class="mt-1 text-sm text-base-content/60">{{ $description }}</p>
    @endif
</a>
