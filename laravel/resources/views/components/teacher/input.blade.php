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
    {{ $attributes->merge(['class' => 'input input-bordered w-full text-sm ' . ($hasError ? 'input-error' : '')]) }}
/>
