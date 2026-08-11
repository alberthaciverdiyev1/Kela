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
    class="mx-auto max-w-4xl space-y-6"
    id="quiz-editor"
    data-content-id="{{ $contentId }}"
    x-data="quizEditor({{ \Illuminate\Support\Js::from($editorConfig) }})"
    @keydown.escape.window="showQuestion = false; showBank = false"
>
    <x-teacher.heading subtitle="Quiz redaktoru — sual əlavə et, düzləndir, sırala">
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
                <button id="add-question-btn" type="button" class="btn btn-sm btn-primary" @click="openAdd()">
                    <x-icon name="heroicon-o-plus-circle" class="size-4" /> Sual Əlavə Et
                </button>
                <button id="bank-btn" type="button" class="btn btn-sm btn-ghost border border-base-300" @click="openBank()">
                    <x-icon name="heroicon-o-banknotes" class="size-4" /> Bankdan Seç
                </button>
            </div>
        </div>

        <div id="questions-list" x-ref="questionsList" @click="onListClick($event)">
            @include('teacher.quizzes._questions', ['contentId' => $contentId, 'questions' => $questions])
        </div>
    </div>

    {{-- Question dialog (add / edit) --}}
    <div x-show="showQuestion" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 x-text="questionTitle" class="mb-4 text-lg font-semibold text-base-content">Sual Əlavə Et</h3>
            <div class="grid gap-4">
                <x-teacher.field label="Sual" name="q_text" :required="true">
                    <x-teacher.input name="q_text" x-model="qText" />
                </x-teacher.field>

                <div class="grid gap-4 sm:grid-cols-2">
                    <x-teacher.field label="A" name="q_option_a" :required="true">
                        <x-teacher.input name="q_option_a" x-model="qOptionA" />
                    </x-teacher.field>
                    <x-teacher.field label="B" name="q_option_b" :required="true">
                        <x-teacher.input name="q_option_b" x-model="qOptionB" />
                    </x-teacher.field>
                    <x-teacher.field label="C" name="q_option_c">
                        <x-teacher.input name="q_option_c" x-model="qOptionC" />
                    </x-teacher.field>
                    <x-teacher.field label="D" name="q_option_d">
                        <x-teacher.input name="q_option_d" x-model="qOptionD" />
                    </x-teacher.field>
                    <x-teacher.field label="E" name="q_option_e">
                        <x-teacher.input name="q_option_e" x-model="qOptionE" />
                    </x-teacher.field>
                    <x-teacher.field label="Doğru cavab" name="q_correct_option">
                        <select name="q_correct_option" x-model="qCorrectOption" class="select select-bordered w-full text-sm">
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

    {{-- Bank dialog --}}
    <div x-show="showBank" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
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
