@props([
    'padding' => true,
])
<div {{ $attributes->merge(['class' => 'rounded-xl border border-base-300 bg-base-100 shadow-sm']) }}>
    @if ($padding)
        <div class="p-6">{{ $slot }}</div>
    @else
        {{ $slot }}
    @endif
</div>
