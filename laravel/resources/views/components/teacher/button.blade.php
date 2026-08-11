@props([
    'variant' => 'primary',
    'size' => 'md',
    'href' => null,
    'type' => 'button',
    'icon' => null,
])

@php
    $variants = [
        'primary' => 'btn-primary text-white',
        'secondary' => 'border border-base-300 bg-white text-base-content shadow-sm hover:bg-base-200',
        'danger' => 'btn-error',
        'ghost' => 'btn-ghost',
    ];
    $sizes = ['xs' => 'btn-xs', 'sm' => 'btn-sm', 'md' => 'btn-md'];
    $classes = 'btn inline-flex items-center gap-2 font-medium '.($variants[$variant] ?? 'btn-primary').' '.($sizes[$size] ?? '');
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
