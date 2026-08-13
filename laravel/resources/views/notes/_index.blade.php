@php
    $noteConfig = [];
@endphp
<div class="space-y-6" x-data="notesApp({{ \Illuminate\Support\Js::from($noteConfig) }})">
    <x-teacher.heading subtitle="Tez bir zamanda qeyd al, rənglə, sabitlə — Google Keep üslubunda">
        Qeydlər
        <x-slot:actions>
            <button
                type="button"
                class="btn btn-sm"
                :class="showTrash ? 'btn-primary' : 'btn-outline'"
                @click="toggleTrash()"
            >
                <x-icon name="heroicon-o-trash" class="size-4" />
                <span x-text="showTrash ? 'Qeydlər' : 'Çöp qutusu'"></span>
            </button>
            <span class="inline-flex items-center gap-1 text-xs font-semibold text-success" x-show="saveState === 'saved'">
                <x-icon name="heroicon-o-check-circle" class="size-4" /> Saxlanıldı
            </span>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Tez qeyd qutusu (Google Keep composer) --}}
    <div class="mx-auto max-w-2xl">
        <div
            class="overflow-hidden rounded-xl border border-base-300 shadow-sm transition"
            :style="'background-color:' + colorOf(composerColor).bg + '; color:#1f2937;'"
        >
            <div class="p-4">
                <template x-if="composerOpen">
                    <input
                        x-model="composerTitle"
                        type="text"
                        placeholder="Başlıq..."
                        @keydown.enter.prevent="closeComposer()"
                        class="mb-1 w-full bg-transparent text-base font-bold text-gray-900 outline-none placeholder:text-gray-400"
                    />
                </template>
                <textarea
                    x-model="composerBody"
                    :rows="composerOpen ? 3 : 1"
                    @click="openComposer()"
                    @focus="openComposer()"
                    @keydown.enter="composerKeydown($event)"
                    placeholder="Qeyd yaz..."
                    class="w-full resize-none bg-transparent text-sm leading-relaxed text-gray-900 outline-none placeholder:text-gray-400"
                ></textarea>

                <div x-show="composerOpen" class="mt-2 flex items-center justify-between">
                    <div class="flex items-center gap-1.5">
                        <template x-for="key in colorKeys()" :key="key">
                            <button
                                type="button"
                                @click="composerColor = key"
                                class="size-5 rounded-full border border-black/10 transition hover:scale-110"
                                :class="composerColor === key && 'ring-2 ring-gray-800/40 ring-offset-1'"
                                :style="swatchStyle(key)"
                                :title="colorOf(key).label"
                            ></button>
                        </template>
                    </div>
                    <button type="button" class="btn btn-ghost btn-sm text-gray-700" @click="closeComposer()">
                        <x-icon name="heroicon-o-x-mark" class="size-4" /> Bağla
                    </button>
                </div>
            </div>
        </div>
    </div>

    {{-- Qeyd şəbəkəsi (masonry grid) --}}
    <template x-if="!showTrash">
        <div>
            <template x-if="!loading && notes.length === 0">
                <x-teacher.empty-state icon="document-plus" title="Hələ qeyd yoxdur" description="Yuxarıdakı qutuya yazaraq ilk qeydinizi yaradın." />
            </template>

            <div class="columns-1 gap-3 sm:columns-2 lg:columns-3 xl:columns-4">
                <template x-for="note in sortedNotes()" :key="note.id">
                    <div
                        class="group mb-3 break-inside-avoid cursor-pointer rounded-xl border border-base-300 p-4 shadow-sm transition hover:shadow-md"
                        :style="cardStyle(note)"
                        @click="openEditor(note)"
                    >
                        <div class="flex items-start justify-between gap-2">
                            <h3 class="break-words text-sm font-bold leading-snug text-gray-900" x-show="note.title" x-text="note.title"></h3>
                            <button
                                type="button"
                                @click.stop="togglePin(note)"
                                class="shrink-0 rounded-full p-1 text-gray-600/50 transition hover:bg-black/10 hover:text-gray-900"
                                :title="note.is_pinned ? 'Sabitdən çıxar' : 'Sabitlə'"
                            >
                                <svg x-show="!note.is_pinned" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" class="size-4">
                                    <path d="M9 4h6v3.5l1.9 2.8a1 1 0 0 1-.84 1.55H7.94a1 1 0 0 1-.84-1.55L9 7.5V4Z" />
                                    <path d="M12 12v8" />
                                </svg>
                                <svg x-show="note.is_pinned" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="size-4">
                                    <path d="M9 4h6v3.5l1.9 2.8a1 1 0 0 1-.84 1.55H7.94a1 1 0 0 1-.84-1.55L9 7.5V4Z" />
                                    <path d="M12 12v8" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
                                </svg>
                            </button>
                        </div>
                        <p class="mt-1 whitespace-pre-wrap break-words text-sm leading-relaxed text-gray-800" x-show="note.body" x-text="note.body"></p>

                        {{-- Hover aksiya çubuğu --}}
                        <div class="mt-3 flex items-center justify-between border-t border-black/10 pt-2 opacity-0 transition group-hover:opacity-100">
                            <div class="flex items-center gap-1">
                                <template x-for="key in colorKeys()" :key="key">
                                    <button
                                        type="button"
                                        @click.stop="setColor(note, key)"
                                        class="size-4 rounded-full border border-black/10 transition hover:scale-110"
                                        :style="swatchStyle(key)"
                                        :title="colorOf(key).label"
                                    ></button>
                                </template>
                            </div>
                            <div class="flex items-center gap-0.5">
                                <button type="button" @click.stop="togglePin(note)" class="rounded-full p-1.5 text-gray-600/60 transition hover:bg-black/10 hover:text-gray-900" title="Sabitlə">
                                    <x-icon name="heroicon-o-paper-clip" class="size-4" />
                                </button>
                                <button type="button" @click.stop="deleteNote(note)" class="rounded-full p-1.5 text-gray-600/60 transition hover:bg-black/10 hover:text-red-600" title="Çöpə at">
                                    <x-icon name="heroicon-o-trash" class="size-4" />
                                </button>
                            </div>
                        </div>
                    </div>
                </template>
            </div>
        </div>
    </template>

    {{-- Çöp qutusu --}}
    <template x-if="showTrash">
        <div>
            <template x-if="trashed.length === 0">
                <x-teacher.empty-state icon="trash" title="Çöp qutusu boşdur" description="Silinən qeydlər burada görünür və geri qaytarıla bilər." />
            </template>
            <div class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-3">
                <template x-for="note in trashed" :key="note.id">
                    <div class="flex items-start justify-between gap-3 rounded-xl border border-base-300 p-4" :style="cardStyle(note)">
                        <div class="min-w-0">
                            <h3 class="truncate text-sm font-bold text-gray-900" x-show="note.title" x-text="note.title"></h3>
                            <p class="mt-1 line-clamp-3 whitespace-pre-wrap text-sm text-gray-700" x-show="note.body" x-text="note.body"></p>
                            <p class="mt-1 text-xs text-gray-500" x-show="!note.title && !note.body">(boş qeyd)</p>
                        </div>
                        <button type="button" class="btn btn-ghost btn-sm shrink-0 text-gray-700" @click="restoreNote(note)" title="Geri qaytar">
                            <x-icon name="heroicon-o-arrow-uturn-left" class="size-4" /> Geri qaytar
                        </button>
                    </div>
                </template>
            </div>
        </div>
    </template>

    {{-- Redaktə modulu --}}
    <template x-if="editing">
        <div
            class="fixed inset-0 z-50 flex items-start justify-center overflow-y-auto bg-black/50 p-4 pt-[8vh]"
            @click.self="closeEditor()"
        >
            <div class="w-full max-w-2xl overflow-hidden rounded-xl border border-base-300 shadow-2xl" :style="cardStyle(editing)">
                <div class="p-5">
                    <input
                        x-model="editing.title"
                        @input="scheduleSave()"
                        type="text"
                        placeholder="Başlıq..."
                        class="w-full bg-transparent text-xl font-bold text-gray-900 outline-none placeholder:text-gray-400"
                    />
                    <textarea
                        x-model="editing.body"
                        @input="scheduleSave()"
                        rows="10"
                        placeholder="Qeyd yaz..."
                        class="mt-2 w-full resize-none bg-transparent text-sm leading-relaxed text-gray-800 outline-none placeholder:text-gray-500"
                    ></textarea>
                </div>
                <div class="flex items-center justify-between gap-3 border-t border-black/10 px-5 py-3">
                    <div class="flex items-center gap-1.5">
                        <template x-for="key in colorKeys()" :key="key">
                            <button
                                type="button"
                                @click="setColor(editing, key)"
                                class="size-6 rounded-full border border-black/10 transition hover:scale-110"
                                :class="editing.color === key && 'ring-2 ring-gray-800/40 ring-offset-1'"
                                :style="swatchStyle(key)"
                                :title="colorOf(key).label"
                            ></button>
                        </template>
                    </div>
                    <div class="flex items-center gap-1">
                        <button type="button" @click="togglePin(editing)" class="rounded-full p-2 text-gray-600 transition hover:bg-black/10 hover:text-gray-900" :title="editing.is_pinned ? 'Sabitdən çıxar' : 'Sabitlə'">
                            <svg x-show="!editing.is_pinned" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" class="size-4">
                                <path d="M9 4h6v3.5l1.9 2.8a1 1 0 0 1-.84 1.55H7.94a1 1 0 0 1-.84-1.55L9 7.5V4Z" />
                                <path d="M12 12v8" />
                            </svg>
                            <svg x-show="editing.is_pinned" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" class="size-4">
                                <path d="M9 4h6v3.5l1.9 2.8a1 1 0 0 1-.84 1.55H7.94a1 1 0 0 1-.84-1.55L9 7.5V4Z" />
                                <path d="M12 12v8" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" />
                            </svg>
                        </button>
                        <button type="button" @click="deleteNote(editing)" class="rounded-full p-2 text-gray-600 transition hover:bg-black/10 hover:text-red-600" title="Çöpə at">
                            <x-icon name="heroicon-o-trash" class="size-4" />
                        </button>
                        <button type="button" class="btn btn-ghost btn-sm text-gray-700" @click="closeEditor()">
                            <x-icon name="heroicon-o-x-mark" class="size-4" /> Bağla
                        </button>
                    </div>
                </div>
            </div>
        </div>
    </template>
</div>
