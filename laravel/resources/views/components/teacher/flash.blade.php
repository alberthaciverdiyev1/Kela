@php
    $flash = match (true) {
        session('success') !== null => ['type' => 'success', 'message' => session('success')],
        session('error') !== null => ['type' => 'error', 'message' => session('error')],
        session('status') !== null => ['type' => 'info', 'message' => session('status')],
        default => null,
    };
@endphp
@if ($flash)
    <div x-data="{ show: true }" x-show="show" x-transition.opacity x-init="setTimeout(() => show = false, 5000)"
         @class([
             'alert mb-4 shadow-sm',
             'alert-success' => $flash['type'] === 'success',
             'alert-error' => $flash['type'] === 'error',
             'alert-info' => $flash['type'] === 'info',
         ])>
        <span class="text-sm">{{ $flash['message'] }}</span>
        <button type="button" class="btn btn-xs btn-ghost" @click="show = false">
            <x-icon name="heroicon-o-x-mark" class="size-4" />
        </button>
    </div>
@endif
