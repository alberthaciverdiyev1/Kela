@props(['nodeId', 'name', 'isFolder' => false])

<div class="flex items-center justify-end gap-1">
    {{-- Rename --}}
    <button
        data-node-action="rename"
        data-node-id="{{ $nodeId }}"
        data-node-name="{{ $name }}"
        title="Adını dəyiş"
        class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
    >
        <x-icon name="heroicon-o-pencil-square" class="size-4" />
    </button>

    {{-- Move --}}
    <button
        data-node-action="move"
        data-node-id="{{ $nodeId }}"
        title="Daşı"
        class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
    >
        <x-icon name="heroicon-o-arrows-right-left" class="size-4" />
    </button>

    {{-- Delete --}}
    <button
        data-node-action="delete"
        data-node-id="{{ $nodeId }}"
        data-node-name="{{ $name }}"
        title="Sil"
        class="rounded-lg p-1.5 text-error/70 hover:bg-error/10 hover:text-error"
    >
        <x-icon name="heroicon-o-trash" class="size-4" />
    </button>
</div>
