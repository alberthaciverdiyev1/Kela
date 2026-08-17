@php
    $flash = match (true) {
        session('success') !== null => ['type' => 'success', 'message' => session('success')],
        session('error') !== null => ['type' => 'error', 'message' => session('error')],
        session('status') !== null => ['type' => 'info', 'message' => session('status')],
        default => null,
    };
@endphp
@if ($flash)
    <div x-data="{ show: true }" x-show="show" x-transition.opacity.duration.300ms x-init="setTimeout(() => show = false, 5000)"
         @class([
             'mb-4 flex items-center gap-3 rounded-2xl border p-4 shadow-sm',
             'border-success/25 bg-success/10' => $flash['type'] === 'success',
             'border-error/25 bg-error/10' => $flash['type'] === 'error',
             'border-info/25 bg-info/10' => $flash['type'] === 'info',
         ])>
        <x-icon :name="'heroicon-o-'.($flash['type'] === 'success' ? 'check-circle' : ($flash['type'] === 'error' ? 'x-circle' : 'information-circle'))"
                :class="'size-5 shrink-0 text-'.$flash['type']" />
        <span class="text-sm font-medium text-base-content/80">{{ $flash['message'] }}</span>
        <button type="button" class="ms-auto rounded-lg p-1 text-base-content/40 transition hover:bg-base-300/40 hover:text-base-content" @click="show = false">
            <x-icon name="heroicon-o-x-mark" class="size-4" />
        </button>
    </div>
@endif
