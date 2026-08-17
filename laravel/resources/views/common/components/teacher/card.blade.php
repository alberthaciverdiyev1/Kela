@props([
    'padding' => true,
    'accent' => null,
])
<div {{ $attributes->merge(['class' => 'relative overflow-hidden rounded-2xl border border-base-300/80 bg-base-100 shadow-sm shadow-base-300/20 transition-shadow duration-200 hover:shadow-md']) }}>
    @if ($accent)
        <div class="h-1 w-full bg-gradient-to-r {{ $accent }}"></div>
    @endif
    @if ($padding)
        <div class="p-6">{{ $slot }}</div>
    @else
        {{ $slot }}
    @endif
</div>
