@props([
    'name' => null,
    'options' => [],
    'placeholder' => null,
    'selected' => null,
])

@php $hasError = $name && $errors->has($name); @endphp

<select
    id="{{ $name }}"
    name="{{ $name }}"
    {{ $attributes->merge(['class' => 'select select-bordered w-full rounded-xl text-sm transition focus:border-primary focus:outline-none focus:ring-2 focus:ring-primary/20 ' . ($hasError ? 'select-error' : '')]) }}
>
    @if ($placeholder)
        <option value="">{{ $placeholder }}</option>
    @endif
    @foreach ($options as $value => $label)
        <option value="{{ $value }}" @selected((string) $selected === (string) $value)>{{ $label }}</option>
    @endforeach
</select>
