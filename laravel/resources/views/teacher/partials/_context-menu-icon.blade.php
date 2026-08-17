{{-- Kontekst menyusu ikonası: item.icon açarına görə heroicon çəkir.
     Daxil olduğu yerdə item Alpine scope-da olmalıdır (x-for daxilində). --}}
<span class="flex size-6 shrink-0 items-center justify-center rounded-md" :class="item.iconClass">
    <template x-if="item.icon === 'pencil-square'"><x-icon name="heroicon-o-pencil-square" class="size-3.5" /></template>
    <template x-if="item.icon === 'arrows-right-left'"><x-icon name="heroicon-o-arrows-right-left" class="size-3.5" /></template>
    <template x-if="item.icon === 'arrow-up-tray'"><x-icon name="heroicon-o-arrow-up-tray" class="size-3.5" /></template>
    <template x-if="item.icon === 'trash'"><x-icon name="heroicon-o-trash" class="size-3.5" /></template>
    <template x-if="item.icon === 'eye'"><x-icon name="heroicon-o-eye" class="size-3.5" /></template>
    <template x-if="item.icon === 'clipboard-document-list'"><x-icon name="heroicon-o-clipboard-document-list" class="size-3.5" /></template>
    <template x-if="item.icon === 'video-camera'"><x-icon name="heroicon-o-video-camera" class="size-3.5" /></template>
    <template x-if="item.icon === 'building-office-2'"><x-icon name="heroicon-o-building-office-2" class="size-3.5" /></template>
    <template x-if="item.icon === 'question-mark-circle'"><x-icon name="heroicon-o-question-mark-circle" class="size-3.5" /></template>
    <template x-if="item.icon === 'user-minus'"><x-icon name="heroicon-o-user-minus" class="size-3.5" /></template>
</span>
