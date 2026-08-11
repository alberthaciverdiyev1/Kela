@props([
    'color' => 'gray',
])

@php
    $colors = [
        'gray' => 'border border-base-300 bg-base-200/60 text-base-content/70',
        'green' => 'badge-success',
        'yellow' => 'badge-warning',
        'red' => 'badge-error',
        'blue' => 'badge-info',
        'primary' => 'badge-primary',
    ];
@endphp

<span {{ $attributes->merge(['class' => 'badge badge-sm font-medium '.($colors[$color] ?? $colors['gray'])]) }}>
    {{ $slot }}
</span>
