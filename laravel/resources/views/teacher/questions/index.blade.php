@extends('common.layouts.teacher')
@section('title', 'Sual Bankı - Kela')
@section('content')
@php
    $questionConfig = [
        'fragmentUrl' => $fragmentUrl,
        'folderId' => $folderId,
        'folderTree' => $folderTree,
    ];
@endphp
<div
    class="space-y-6"
    x-data="questionBank({{ \Illuminate\Support\Js::from($questionConfig) }})"
    @keydown.escape.window="closeAll()"
>
    <x-teacher.heading subtitle="Sualları qovluqlara bölərək təşkil et">
        Sual Bankı
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.quizzes.index') }}" variant="ghost" icon="clipboard-document-list">Quizlər</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Toolbar --}}
    <div class="flex flex-wrap items-center gap-2">
        <button type="button" class="btn btn-sm btn-ghost border border-base-300" @click="openFolderAdd()">
            <x-icon name="heroicon-o-folder-plus" class="size-4" /> Yeni Qovluq
        </button>
        <button type="button" class="btn btn-sm btn-primary" @click="openQuestionAdd()">
            <x-icon name="heroicon-o-plus-circle" class="size-4" /> Yeni Sual
        </button>
    </div>

    {{-- Kataloq (JS fragment ilə yenilənir) --}}
    <div id="question-bank" x-ref="directory" @contextmenu="onTableContextMenu($event)">
        @include('teacher.questions._table', [
            'folderId' => $folderId,
            'folders' => $folders,
            'questions' => $questions,
            'breadcrumbs' => $breadcrumbs,
            'folderTree' => $folderTree,
        ])
    </div>

    {{-- Yeni Qovluq dialog --}}
    <div x-show="showFolderAdd" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Yeni Qovluq</h3>
            <input
                x-ref="folderNameInput"
                type="text"
                placeholder="Qovluq adı"
                class="input input-bordered w-full text-sm"
            />
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showFolderAdd = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveFolder()">Yarat</button>
            </div>
        </div>
    </div>

    {{-- Qovluq adını dəyiş dialog --}}
    <div x-show="showFolderRename" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Qovluq adını dəyiş</h3>
            <input
                x-ref="folderRenameInput"
                type="text"
                placeholder="Yeni ad"
                class="input input-bordered w-full text-sm"
            />
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showFolderRename = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveFolderRename()">Saxla</button>
            </div>
        </div>
    </div>

    {{-- Qovluq daşı dialog --}}
    <div x-show="showFolderMove" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Qovluğu daşı</h3>
            <select x-ref="folderMoveSelect" class="select select-bordered w-full text-sm">
                <option value="">Kökə</option>
                <template x-for="(f, i) in folderTree" :key="f.id">
                    <option :value="f.id" x-text="'&nbsp;'.repeat(f.depth) + f.name"></option>
                </template>
            </select>
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showFolderMove = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveFolderMove()">Daşı</button>
            </div>
        </div>
    </div>

    {{-- Sual əlavə et / düzləndir dialog (canlı önizləmə ilə) --}}
    <div x-show="showQuestion" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="max-h-[90vh] w-full max-w-3xl overflow-y-auto rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <div class="flex items-center justify-between gap-4 border-b border-base-300 pb-3">
                <h3 x-text="questionTitle" class="text-lg font-semibold text-base-content">Yeni Sual</h3>
                <button type="button" class="rounded-lg p-1 text-base-content/50 hover:bg-base-200 hover:text-base-content" @click="showQuestion = false" title="Bağla">
                    <x-icon name="heroicon-o-x-mark" class="size-5" />
                </button>
            </div>

            <div class="mt-4 grid gap-6 lg:grid-cols-5">
                {{-- Form (sol) --}}
                <div class="space-y-4 lg:col-span-3">
                    <x-teacher.field label="Sual" :required="true">
                        <div class="overflow-hidden rounded-lg border border-base-300 bg-base-100 focus-within:border-primary">
                            {{-- Rich text toolbar --}}
                            <div class="flex flex-wrap items-center gap-0.5 border-b border-base-300 bg-base-200/40 px-1.5 py-1">
                                <button type="button" @click="execRich('bold')" title="Qalın" class="rounded px-2 py-1 text-sm font-bold text-base-content/70 hover:bg-base-300 hover:text-base-content">B</button>
                                <button type="button" @click="execRich('italic')" title="İtalik" class="rounded px-2 py-1 text-sm italic text-base-content/70 hover:bg-base-300 hover:text-base-content">I</button>
                                <button type="button" @click="execRich('underline')" title="Altı xətt" class="rounded px-2 py-1 text-sm underline text-base-content/70 hover:bg-base-300 hover:text-base-content">U</button>
                                <button type="button" @click="execRich('strikeThrough')" title="Üstündən xətt" class="rounded px-2 py-1 text-sm line-through text-base-content/70 hover:bg-base-300 hover:text-base-content">S</button>
                                <span class="mx-1 h-4 w-px bg-base-300"></span>
                                <button type="button" @click="execRich('insertUnorderedList')" title="Liste" class="rounded px-2 py-1 text-sm text-base-content/70 hover:bg-base-300 hover:text-base-content">• Liste</button>
                                <button type="button" @click="execRich('insertOrderedList')" title="Sıralı liste" class="rounded px-2 py-1 text-sm text-base-content/70 hover:bg-base-300 hover:text-base-content">1. Liste</button>
                            </div>
                            <div
                                x-ref="qFormText"
                                contenteditable="true"
                                data-placeholder="Sual mətnini yazın..."
                                class="rich-editor-input min-h-[120px] p-3 text-sm leading-relaxed text-base-content focus:outline-none"
                            ></div>
                        </div>
                        <p class="mt-1 text-xs text-base-content/40">Mətni formatlaya bilərsiniz — qalın, italik, altı xətt, listelər.</p>
                    </x-teacher.field>

                    <div>
                        <div class="mb-1 flex items-center justify-between">
                            <label class="label py-0 font-medium text-base-content/70">Seçimlər</label>
                            <span class="text-xs text-base-content/40">Doğru cavabı işarələyin</span>
                        </div>
                        <div class="space-y-2">
                            @foreach ([['A', 'option_a', 0], ['B', 'option_b', 1], ['C', 'option_c', 2], ['D', 'option_d', 3], ['E', 'option_e', 4]] as [$letter, $field, $index])
                                <div class="flex items-center gap-2">
                                    <input
                                        type="radio"
                                        name="q_correct"
                                        value="{{ $index }}"
                                        x-model="qForm.correct_option"
                                        class="radio radio-primary radio-sm"
                                        :disabled="qForm.{{ $field }}.trim() === ''"
                                        title="Doğru cavab kimi işarələ"
                                    />
                                    <span class="w-4 shrink-0 font-bold text-base-content/60">{{ $letter }}.</span>
                                    <input
                                        type="text"
                                        placeholder="Seçim {{ $letter }}"
                                        x-model="qForm.{{ $field }}"
                                        class="input input-bordered w-full text-sm"
                                    />
                                </div>
                            @endforeach
                        </div>
                    </div>

                    <x-teacher.field label="İzah (isteğe bağlı)" name="explanation">
                        <textarea x-model="qForm.explanation" rows="2" placeholder="Cavabı şərh edin — tələbələrə kömək edər..." class="textarea textarea-bordered w-full text-sm"></textarea>
                    </x-teacher.field>
                </div>

                {{-- Canlı önizləmə (sağ) --}}
                <div class="lg:col-span-2">
                    <div class="rounded-xl border border-base-300 bg-base-200/40 p-4">
                        <p class="mb-3 text-xs font-semibold uppercase tracking-wide text-base-content/40">Önizləmə</p>
                        <div class="rounded-lg border border-base-300 bg-base-100 p-4 shadow-sm">
                            <div class="mb-2 min-h-[1.5rem] text-sm font-semibold leading-snug text-base-content">
                                <div class="rich-preview" x-html="qForm.text"></div>
                                <p x-show="!qForm.text" class="text-base-content/40">Sual mətni...</p>
                            </div>
                            <div class="space-y-1">
                                <template x-for="(opt, i) in previewOptions()" :key="i">
                                    <div
                                        class="flex items-center gap-2 rounded px-2 py-1 text-sm"
                                        :class="opt.isCorrect ? 'bg-green-50 text-green-800 ring-1 ring-green-200' : 'text-base-content/80'"
                                    >
                                        <span
                                            class="inline-flex size-5 shrink-0 items-center justify-center rounded-full text-[10px] font-bold"
                                            :class="opt.isCorrect ? 'bg-green-600 text-white' : 'bg-base-200 text-base-content/60'"
                                            x-text="opt.letter"
                                        ></span>
                                        <span x-text="opt.text"></span>
                                    </div>
                                </template>
                                <p x-show="previewOptions().length === 0" class="py-1 text-xs text-base-content/40">Seçim əlavə edin...</p>
                            </div>
                            <div x-show="qForm.explanation" class="mt-3 rounded bg-amber-50 px-2 py-1.5 text-xs text-amber-800">
                                <span class="font-semibold">İzah:</span> <span x-text="qForm.explanation"></span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="mt-5 flex items-center justify-end gap-2 border-t border-base-300 pt-4">
                <button type="button" class="btn btn-sm btn-ghost" @click="showQuestion = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveQuestion()">Saxla</button>
            </div>
        </div>
    </div>

    {{-- Sual daşı dialog --}}
    <div x-show="showQuestionMove" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Sualı qovluğa daşı</h3>
            <select x-ref="questionMoveSelect" class="select select-bordered w-full text-sm">
                <option value="">Kökə</option>
                <template x-for="(f, i) in folderTree" :key="f.id">
                    <option :value="f.id" x-text="'&nbsp;'.repeat(f.depth) + f.name"></option>
                </template>
            </select>
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showQuestionMove = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveQuestionMove()">Daşı</button>
            </div>
        </div>
    </div>

    {{-- Sağ-tık kontekst menyusu --}}
    @include('teacher.partials._context-menu')
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/question/controller.js')
@endpush
