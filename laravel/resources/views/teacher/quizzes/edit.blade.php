@extends('common.layouts.teacher')
@section('title', 'Quiz Redaktoru - Kela')
@section('content')
@php
    $editorConfig = [
        'quizId' => (int) $contentId,
        'fragmentUrl' => route('teacher.quizzes.questions', $contentId),
        'questionCount' => count($questions),
    ];
@endphp
<div
    class="space-y-6"
    id="quiz-editor"
    data-content-id="{{ $contentId }}"
    x-data="quizEditor({{ \Illuminate\Support\Js::from($editorConfig) }})"
    @keydown.escape.window="showBank = false"
>
    <x-teacher.heading subtitle="Quiz redaktoru — bankdan sual seç, sırala, çıxar">
        {{ $quiz['title'] ?? 'Quiz Redaktoru' }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.quizzes.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Quiz form --}}
    <x-teacher.card>
        <form method="POST" action="{{ route('teacher.quizzes.update', $contentId) }}" class="grid gap-5">
            @csrf

            <x-teacher.field label="Başlıq" name="title" :required="true">
                <x-teacher.input name="title" value="{{ old('title', $quiz['title'] ?? '') }}" />
            </x-teacher.field>

            <x-teacher.field label="Təsvir" name="description">
                <x-teacher.textarea name="description" rows="3">{{ old('description', $quiz['description'] ?? '') }}</x-teacher.textarea>
            </x-teacher.field>

            <x-teacher.field label="Qovluq" name="folder_id" hint="Boş buraxsanız quiz kökdə saxlanılır.">
                <select name="folder_id" class="select select-bordered w-full text-sm">
                    <option value="">Kök (qovluq seçilməyib)</option>
                    @foreach ($folderTree as $folder)
                        <option
                            value="{{ $folder['id'] }}"
                            @selected(old('folder_id', $quiz['folder_id'] ?? null) == $folder['id'])
                        >
                            {{ str_repeat('— ', $folder['depth']) }}{{ $folder['name'] }}
                        </option>
                    @endforeach
                </select>
            </x-teacher.field>

            <x-teacher.field label="Yayımlandı" name="is_published">
                <label class="flex items-center gap-2 pt-2">
                    <input
                        type="checkbox"
                        name="is_published"
                        value="1"
                        @checked(old('is_published', $quiz['is_published'] ?? false))
                        class="toggle toggle-primary"
                    />
                    <span class="text-sm text-base-content/70">Yayımlanmış quiz kimi işarələ</span>
                </label>
            </x-teacher.field>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 pt-5">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </form>
    </x-teacher.card>

    {{-- Question list --}}
    <div class="space-y-3">
        <div class="flex flex-wrap items-center justify-between gap-2">
            <h3 class="flex items-center gap-2 text-lg font-semibold text-base-content">
                Quiz Sualları
                <x-teacher.badge color="blue" x-text="questionCount">0</x-teacher.badge>
            </h3>
            <div class="flex flex-wrap items-center gap-2">
                {{-- Sual yaratma burada DEYİL — Sual Bankı modulundadır (teacher/questions).
                     Burada yalnız bankdan seçib quizə bağlamaq var. --}}
                <button id="bank-btn" type="button" class="btn btn-sm btn-primary" @click="openBank()">
                    <x-icon name="heroicon-o-banknotes" class="size-4" /> Bankdan Seç
                </button>
                <a href="{{ route('teacher.questions.index') }}" class="btn btn-sm btn-ghost border border-base-300">
                    <x-icon name="heroicon-o-plus-circle" class="size-4" /> Sual Bankına Get
                </a>
            </div>
        </div>

        <div id="questions-list" x-ref="questionsList" @click="onListClick($event)">
            @include('teacher.quizzes._questions', ['contentId' => $contentId, 'questions' => $questions])
        </div>
    </div>

    {{-- Bank dialog --}}
    <div x-show="showBank" x-cloak x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Bankdan Seç</h3>
            <p class="mb-3 text-sm text-base-content/60">Sual bankından mövcud sualı quizə əlavə edin.</p>
            @if (count($bankOptions) === 0)
                <p class="text-sm text-base-content/50">Bankda əlavə edilə bilən sual yoxdur.</p>
            @else
                <select name="bank_question_id" x-model="bankQuestionId" class="select select-bordered w-full text-sm">
                    <option value="">Sual seçin...</option>
                    @foreach ($bankOptions as $value => $label)
                        <option value="{{ $value }}">{{ $label }}</option>
                    @endforeach
                </select>
            @endif
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showBank = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="addFromBank()">Əlavə Et</button>
            </div>
        </div>
    </div>
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/quiz-editor/index.js')
@endpush
