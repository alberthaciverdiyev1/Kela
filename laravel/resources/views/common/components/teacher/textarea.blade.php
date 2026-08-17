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
    {{ $attributes->merge(['class' => 'textarea textarea-bordered w-full rounded-xl text-sm transition focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 ' . ($hasError ? 'textarea-error' : '')]) }}
>{{ $slot }}</textarea>
