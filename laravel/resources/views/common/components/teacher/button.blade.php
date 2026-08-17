@props([
    'variant' => 'primary',
    'size' => 'md',
    'href' => null,
    'type' => 'button',
    'icon' => null,
])

@php
    $variants = [
        'primary' => 'border-0 bg-primary text-white shadow-sm hover:bg-primary/90',
        'secondary' => 'border border-base-300 bg-base-100 text-base-content shadow-sm hover:bg-base-200/50 hover:border-base-300',
        'outline' => 'border border-base-300 bg-transparent text-base-content/80 hover:border-base-300 hover:bg-base-200/50 hover:text-base-content',
        'danger' => 'btn-error text-white hover:brightness-110',
        'ghost' => 'btn-ghost text-base-content/70 hover:bg-base-200/50 hover:text-base-content',
    ];
    $sizes = ['xs' => 'btn-xs', 'sm' => 'btn-sm', 'md' => 'btn-md'];
    $classes = 'btn inline-flex items-center gap-2 font-medium transition-all duration-200 '.($variants[$variant] ?? $variants['primary']).' '.($sizes[$size] ?? '');
@endphp

@if ($href)
    <a href="{{ $href }}" {{ $attributes->merge(['class' => $classes]) }}>
        @if ($icon)<x-icon name="heroicon-o-{{ $icon }}" class="size-4" />@endif
        {{ $slot }}
    </a>
@else
    <button type="{{ $type }}" {{ $attributes->merge(['class' => $classes]) }}>
        @if ($icon)<x-icon name="heroicon-o-{{ $icon }}" class="size-4" />@endif
        {{ $slot }}
    </button>
@endif
