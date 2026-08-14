{{-- Sağ-tık kontekst menyusu — attendance popover üslubunda.
     Controller-ə createContextMenu() mixini əridilmiş olmalıdır. --}}
<template x-if="ctxMenu.show">
    <div>
        {{-- Arxa fon: klik və ya sağ-tık menyunu bağlayır --}}
        <div class="fixed inset-0 z-30" @click="closeCtxMenu()" @contextmenu.prevent="closeCtxMenu()"></div>

        <div
            class="fixed z-40 w-64 overflow-hidden rounded-xl border border-base-300 bg-base-100 py-1 shadow-xl"
            :style="'left: ' + ctxMenu.left + 'px; top: ' + ctxMenu.top + 'px'"
            @click.stop
        >
            {{-- Başlıq (attendance popover ilə eyni üslub) --}}
            <p class="border-b border-base-200 px-3 py-2 text-[11px] font-semibold uppercase tracking-wide text-base-content/50" x-text="ctxMenu.title"></p>

            <div class="max-h-[65vh] overflow-y-auto overflow-x-hidden py-1">
                <template x-for="(item, i) in ctxMenu.items" :key="i">
                    <div>
                        {{-- Ayırıcı --}}
                        <div x-show="item.divider" class="my-1 border-t border-base-200"></div>

                        {{-- Link elementi (href) --}}
                        <a
                            x-show="!item.divider && item.href"
                            :href="item.href"
                            :target="item.target || '_self'"
                            @click="closeCtxMenu()"
                            class="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium transition"
                            :class="item.cls || (item.danger ? 'text-error hover:bg-error/10 hover:text-error' : 'text-base-content hover:bg-base-200')"
                        >
                            @include('teacher.partials._context-menu-icon', ['icon' => ''])
                            <span class="flex-1" x-text="item.label"></span>
                        </a>

                        {{-- Aksiya düyməsi (action) --}}
                        <button
                            x-show="!item.divider && !item.href"
                            type="button"
                            @click="runCtxItem(item)"
                            class="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium transition"
                            :class="item.cls || (item.danger ? 'text-error hover:bg-error/10 hover:text-error' : 'text-base-content hover:bg-base-200')"
                        >
                            @include('teacher.partials._context-menu-icon', ['icon' => ''])
                            <span class="flex-1" x-text="item.label"></span>
                        </button>
                    </div>
                </template>
            </div>
        </div>
    </div>
</template>
