@props([
    'color' => 'gray',
])

@php
    $colors = [
        'gray' => 'border border-base-300 bg-base-200/70 text-base-content/70',
        'green' => 'border border-success/20 bg-success/10 text-success',
        'yellow' => 'border border-warning/20 bg-warning/10 text-warning',
        'red' => 'border border-error/20 bg-error/10 text-error',
        'blue' => 'border border-info/20 bg-info/10 text-info',
        'primary' => 'border border-primary/20 bg-primary/10 text-primary',
    ];
@endphp

<span {{ $attributes->merge(['class' => 'badge badge-sm gap-1 font-medium '.($colors[$color] ?? $colors['gray'])]) }}>
    {{ $slot }}
</span>
