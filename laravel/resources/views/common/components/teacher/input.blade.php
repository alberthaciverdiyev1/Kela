@props([
    'name' => null,
    'type' => 'text',
    'placeholder' => null,
])

@php $hasError = $name && $errors->has($name); @endphp

<input
    id="{{ $name }}"
    name="{{ $name }}"
    type="{{ $type }}"
    placeholder="{{ $placeholder }}"
    {{ $attributes->merge(['class' => 'input input-bordered w-full rounded-xl text-sm transition focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 ' . ($hasError ? 'input-error' : '')]) }}
/>
