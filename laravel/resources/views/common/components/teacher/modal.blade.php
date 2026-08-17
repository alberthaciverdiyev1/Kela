@props([
    'title' => null,
    'show' => 'false',
    'maxWidth' => 'md',
])

@php
    $maxWidthClass = match($maxWidth) {
        'sm' => 'max-w-sm',
        'md' => 'max-w-md',
        'lg' => 'max-w-lg',
        'xl' => 'max-w-xl',
        '2xl' => 'max-w-2xl',
        default => 'max-w-md',
    };
@endphp

<div 
    x-show="{{ $show }}" 
    x-cloak 
    class="fixed inset-0 z-50 flex items-center justify-center p-4 sm:p-6"
    aria-labelledby="modal-title"
    role="dialog"
    aria-modal="true"
>
    {{-- Backdrop --}}
    <div 
        x-show="{{ $show }}"
        x-transition:enter="transition ease-out duration-300"
        x-transition:enter-start="opacity-0"
        x-transition:enter-end="opacity-100"
        x-transition:leave="transition ease-in duration-200"
        x-transition:leave-start="opacity-100"
        x-transition:leave-end="opacity-0"
        class="fixed inset-0 bg-base-content/20 backdrop-blur-sm transition-opacity" 
        @click="{{ $show }} = false"
    ></div>

    {{-- Modal Panel --}}
    <div 
        x-show="{{ $show }}"
        x-transition:enter="transition ease-out duration-300"
        x-transition:enter-start="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
        x-transition:enter-end="opacity-100 translate-y-0 sm:scale-100"
        x-transition:leave="transition ease-in duration-200"
        x-transition:leave-start="opacity-100 translate-y-0 sm:scale-100"
        x-transition:leave-end="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
        class="relative max-h-[90vh] w-full {{ $maxWidthClass }} overflow-y-auto rounded-2xl border border-base-300 bg-base-100 text-left shadow-2xl transition-all"
    >
        @if($title)
            <div class="flex items-center justify-between border-b border-base-200 px-6 py-4">
                <h3 class="text-lg font-semibold text-base-content" id="modal-title">{{ $title }}</h3>
                <button type="button" @click="{{ $show }} = false" class="rounded-lg p-1.5 text-base-content/50 transition hover:bg-base-200 hover:text-base-content">
                    <x-icon name="heroicon-o-x-mark" class="size-5" />
                </button>
            </div>
        @endif
        
        <div class="px-6 py-5">
            {{ $slot }}
        </div>

        @isset($footer)
            <div class="flex items-center justify-end gap-3 border-t border-base-200 bg-base-200/30 px-6 py-4">
                {{ $footer }}
            </div>
        @endisset
    </div>
</div>
