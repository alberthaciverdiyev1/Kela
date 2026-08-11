@props(['nodeId', 'name', 'isFolder' => false])

<div class="flex items-center justify-end gap-1">
    {{-- Rename --}}
    <button
        wire:click="renameNode({{ $nodeId }})"
        title="Adını dəyiş"
        class="rounded-lg p-1.5 text-gray-500 hover:bg-gray-100 hover:text-gray-700"
    >
        <x-heroicon-o-pencil-square class="h-4 w-4" />
    </button>

    {{-- Move --}}
    <button
        wire:click="openMove({{ $nodeId }})"
        title="Daşı"
        class="rounded-lg p-1.5 text-gray-500 hover:bg-gray-100 hover:text-gray-700"
    >
        <x-heroicon-o-arrows-right-left class="h-4 w-4" />
    </button>

    {{-- Delete --}}
    <button
        wire:confirm="'{{ $name }}' silinsin?"
        wire:click="deleteNode({{ $nodeId }})"
        title="Sil"
        class="rounded-lg p-1.5 text-red-500 hover:bg-red-50 hover:text-red-700"
    >
        <x-heroicon-o-trash class="h-4 w-4" />
    </button>
</div>
