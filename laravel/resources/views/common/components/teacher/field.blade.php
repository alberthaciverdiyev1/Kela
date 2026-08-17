@props([
    'label' => null,
    'name' => null,
    'hint' => null,
    'required' => false,
])
<div {{ $attributes->merge(['class' => 'space-y-1.5']) }}>
    @if ($label)
        <label for="{{ $name }}" class="block text-sm font-semibold text-base-content/80">
            {{ $label }}
            @if ($required)<span class="text-error">*</span>@endif
        </label>
    @endif
    {{ $slot }}
    @error($name)
        <p class="text-xs text-error">{{ $message }}</p>
    @enderror
    @if ($hint)
        <p class="text-xs text-base-content/50">{{ $hint }}</p>
    @endif
</div>
