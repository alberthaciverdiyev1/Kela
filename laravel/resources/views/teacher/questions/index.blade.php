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
    <div id="question-bank" x-ref="directory" @click="onTableClick($event)">
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

    {{-- Sual əlavə et / düzləndir dialog --}}
    <div x-show="showQuestion" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 x-text="questionTitle" class="mb-4 text-lg font-semibold text-base-content">Yeni Sual</h3>
            <div class="grid gap-4">
                <x-teacher.field label="Sual" :required="true">
                    <input x-ref="qText" type="text" placeholder="Sual mətni" class="input input-bordered w-full text-sm" />
                </x-teacher.field>

                <div class="grid gap-4 sm:grid-cols-2">
                    <x-teacher.field label="A" :required="true">
                        <input x-ref="qOptionA" type="text" placeholder="Seçim A" class="input input-bordered w-full text-sm" />
                    </x-teacher.field>
                    <x-teacher.field label="B" :required="true">
                        <input x-ref="qOptionB" type="text" placeholder="Seçim B" class="input input-bordered w-full text-sm" />
                    </x-teacher.field>
                    <x-teacher.field label="C">
                        <input x-ref="qOptionC" type="text" placeholder="Seçim C" class="input input-bordered w-full text-sm" />
                    </x-teacher.field>
                    <x-teacher.field label="D">
                        <input x-ref="qOptionD" type="text" placeholder="Seçim D" class="input input-bordered w-full text-sm" />
                    </x-teacher.field>
                    <x-teacher.field label="E">
                        <input x-ref="qOptionE" type="text" placeholder="Seçim E" class="input input-bordered w-full text-sm" />
                    </x-teacher.field>
                    <x-teacher.field label="Doğru cavab">
                        <select x-ref="qCorrectOption" class="select select-bordered w-full text-sm">
                            @foreach ([0 => 'A', 1 => 'B', 2 => 'C', 3 => 'D', 4 => 'E'] as $value => $letter)
                                <option value="{{ $value }}">{{ $letter }}</option>
                            @endforeach
                        </select>
                    </x-teacher.field>
                </div>

                <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-4">
                    <button type="button" class="btn btn-sm btn-ghost" @click="showQuestion = false">Ləğv et</button>
                    <button type="button" class="btn btn-sm btn-primary" @click="saveQuestion()">Saxla</button>
                </div>
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
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/question/controller.js')
@endpush
