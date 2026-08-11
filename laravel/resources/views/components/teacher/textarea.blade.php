@props([
    'name' => null,
    'rows' => 3,
    'placeholder' => null,
])

@php $hasError = $name && $errors->has($name); @endphp

<textarea
    id="{{ $name }}"
    name="{{ $name }}"
    rows="{{ $rows }}"
    placeholder="{{ $placeholder }}"
    {{ $attributes->merge(['class' => 'textarea textarea-bordered w-full text-sm ' . ($hasError ? 'textarea-error' : '')]) }}
>{{ $slot }}</textarea>
