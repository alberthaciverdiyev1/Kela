@extends('common.layouts.teacher')
@section('title', $heading)
@section('content')
@php
    $homeworkConfig = ['questions' => $questions];
@endphp
<div
    class="mx-auto max-w-3xl space-y-6"
    x-data="homeworkEditor({{ \Illuminate\Support\Js::from($homeworkConfig) }})"
    @keydown.escape.window="showQuizPicker = false; showTaskModal = false"
>
    <x-teacher.heading :subtitle="$subtitle">
        {{ $heading }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.homeworks.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    <form
        method="POST"
        action="{{ $creating ? route('teacher.homeworks.store') : route('teacher.homeworks.update', $homework['id']) }}"
        class="space-y-6"
    >
        @csrf
        {{-- Sual kompozisiyası gizli sahə ilə servisə gedir --}}
        <input type="hidden" name="questions_json" x-bind:value="questionsJson" />

        <x-teacher.card>
            <div class="grid gap-5">
                <x-teacher.field label="Başlıq" name="title" :required="true">
                    <x-teacher.input name="title" value="{{ old('title', $homework['title'] ?? '') }}" />
                </x-teacher.field>

                <x-teacher.field label="Təsvir" name="description">
                    <x-teacher.textarea name="description" rows="3">{{ old('description', $homework['description'] ?? '') }}</x-teacher.textarea>
                </x-teacher.field>

                <x-teacher.field label="Yayımlandı" name="is_published">
                    <label class="flex items-center gap-2 pt-2">
                        <input
                            type="checkbox"
                            name="is_published"
                            value="1"
                            @checked(old('is_published', $homework['is_published'] ?? false))
                            class="toggle toggle-primary"
                        />
                        <span class="text-sm text-base-content/70">Yayımlanmış ev tapşırığı kimi işarələ</span>
                    </label>
                </x-teacher.field>
            </div>
        </x-teacher.card>

        {{-- Sual kompozisiyası --}}
        <x-teacher.card :padding="false">
            <div class="flex flex-wrap items-center justify-between gap-3 border-b border-base-300 p-4">
                <div>
                    <h3 class="text-base font-semibold text-base-content">Suallar</h3>
                    <p class="mt-0.5 text-xs text-base-content/50">
                        <span x-text="questions.length"></span> sual —
                        <span x-text="quizCount"></span> quiz sualı ·
                        <span x-text="taskCount"></span> tapşırıq
                    </p>
                </div>
                <div class="flex flex-wrap items-center gap-2">
                    <button type="button" class="btn btn-sm btn-primary" @click="openQuizPicker()">
                        <x-icon name="heroicon-o-clipboard-document-list" class="size-4" /> Quizdən əlavə et
                    </button>
                    <button type="button" class="btn btn-sm btn-ghost border border-base-300" @click="showTaskModal = true">
                        <x-icon name="heroicon-o-pencil" class="size-4" /> Əl ilə tapşırıq yaz
                    </button>
                </div>
            </div>

            <div class="p-4">
                <template x-if="questions.length === 0">
                    <div class="flex flex-col items-center justify-center gap-2 py-10 text-center">
                        <x-icon name="heroicon-o-clipboard-document" class="size-10 text-base-content/30" />
                        <p class="text-sm font-medium text-base-content/70">Hələ sual yoxdur</p>
                        <p class="max-w-sm text-xs text-base-content/50">Yuxarıdakı düymələrlə quizlərdən sual seçin və ya əl ilə tapşırıq yazın.</p>
                    </div>
                </template>

                <template x-for="(q, idx) in questions" :key="idx">
                    <div class="flex items-start gap-3 border-b border-base-200 py-3 last:border-0">
                        {{-- Nömrə --}}
                        <span class="mt-0.5 flex size-6 shrink-0 items-center justify-center rounded-full bg-base-200 text-xs font-bold text-base-content/60" x-text="idx + 1"></span>

                        {{-- Məzmun --}}
                        <div class="min-w-0 flex-1">
                            <div class="flex flex-wrap items-center gap-1.5">
                                <span class="badge badge-sm font-medium" :class="q.type === 1 ? 'badge-info' : 'badge-neutral'">
                                    <span x-text="q.type === 1 ? 'Quiz sualı' : 'Tapşırıq'"></span>
                                </span>
                                <span class="badge badge-sm badge-success" x-show="q.type === 1 && correctLetter(q)" x-text="'Doğru: ' + correctLetter(q)"></span>
                            </div>
                            <p class="rich-preview mt-1 text-sm font-medium text-base-content" x-html="q.text"></p>

                            {{-- Quiz sualının variantları --}}
                            <div class="mt-2 flex flex-wrap gap-1" x-show="q.type === 1">
                                <template x-for="(opt, oi) in itemOptions(q)" :key="oi">
                                    <span class="rounded bg-base-200 px-1.5 py-0.5 text-xs text-base-content/70">
                                        <span class="font-semibold" x-text="opt.letter + '.'"></span> <span x-text="opt.text"></span>
                                    </span>
                                </template>
                            </div>

                            <p class="mt-1 text-xs text-base-content/40" x-show="q.type === 1 && q.source_quiz_id">
                                Mənbə: quiz #<span x-text="q.source_quiz_id"></span>
                            </p>
                        </div>

                        {{-- Əməliyyatlar --}}
                        <div class="flex shrink-0 items-center gap-0.5">
                            <button type="button" @click="move(idx, -1)" :disabled="idx === 0" title="Yuxarı" class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content disabled:opacity-30">
                                <x-icon name="heroicon-o-chevron-up" class="size-4" />
                            </button>
                            <button type="button" @click="move(idx, 1)" :disabled="idx === questions.length - 1" title="Aşağı" class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content disabled:opacity-30">
                                <x-icon name="heroicon-o-chevron-down" class="size-4" />
                            </button>
                            <button type="button" @click="remove(idx)" title="Sil" class="rounded-lg p-1.5 text-error/70 hover:bg-error/10 hover:text-error">
                                <x-icon name="heroicon-o-trash" class="size-4" />
                            </button>
                        </div>
                    </div>
                </template>
            </div>

            <div class="flex items-center justify-end gap-2 border-t border-base-300 px-4 py-3">
                <x-teacher.button type="submit" icon="check">Yadda Saxla</x-teacher.button>
            </div>
        </x-teacher.card>
    </form>

    {{-- Quiz seçimi pəncərəsi --}}
    <div x-show="showQuizPicker" x-cloak x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-lg overflow-hidden rounded-xl border border-base-300 bg-base-100 shadow-xl">
            <div class="flex items-center justify-between gap-4 border-b border-base-200 px-5 py-4">
                <div>
                    <h3 class="text-base font-semibold text-base-content">Quizdən sual əlavə et</h3>
                    <p class="mt-0.5 text-xs text-base-content/50">Quizi seçin, sualları işarələyin və əlavə edin.</p>
                </div>
                <button type="button" @click="showQuizPicker = false" class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200" aria-label="Bağla">
                    <x-icon name="heroicon-o-x-mark" class="size-5" />
                </button>
            </div>

            <div class="space-y-3 border-b border-base-200 px-5 py-4">
                <input
                    type="text"
                    x-model="quizSearch"
                    placeholder="Quiz və ya qovluq axtar..."
                    class="input input-bordered w-full text-sm"
                />
                <div class="flex items-center gap-2 text-xs text-base-content/50" x-show="quizzesLoading">
                    <span class="loading loading-spinner loading-xs text-primary"></span> Quizlər yüklənir...
                </div>
                <div class="flex items-center gap-2 text-xs text-base-content/50" x-show="!quizzesLoading && quizzes.length === 0">
                    Quiz tapılmadı.
                </div>
            </div>

            <div class="max-h-48 overflow-y-auto px-2 py-2">
                {{-- Qovluq başlıqları + içlərindəki quizlər (ic-ice qruplaşmış) --}}
                <template x-for="row in quizRows" :key="row.key">
                    <div class="space-y-0.5">
                        {{-- Qovluq başlığı (tıklanaraq açılır/bağlanır) --}}
                        <div x-show="row.kind === 'folder'"
                            @click="toggleFolder(row.key)"
                            class="flex cursor-pointer select-none items-center gap-1.5 pt-3 pb-1 text-xs font-semibold uppercase tracking-wide text-base-content/50 transition hover:text-base-content/80"
                            :style="'padding-left: ' + (row.depth * 16 + 4) + 'px'">
                            <span x-show="!row.collapsed" class="shrink-0"><x-icon name="heroicon-o-chevron-down" class="size-3.5" /></span>
                            <span x-show="row.collapsed" class="shrink-0"><x-icon name="heroicon-o-chevron-right" class="size-3.5" /></span>
                            <x-icon name="heroicon-o-folder" class="size-4 shrink-0 text-primary/70" />
                            <span class="min-w-0 flex-1 truncate" x-text="row.name"></span>
                            <span class="text-base-content/40" x-text="row.count"></span>
                        </div>

                        {{-- Quiz sətri --}}
                        <div x-show="row.kind === 'quiz'"
                            class="flex cursor-pointer select-none items-center gap-3 rounded-lg px-3 py-2 text-sm transition hover:bg-base-200/60"
                            :class="Number(selectedQuizId) === row.q?.id ? 'bg-primary/10' : ''"
                            :style="'padding-left: ' + (row.depth * 16 + 8) + 'px'"
                            @click="selectQuizRow(row.q)">
                            <x-icon name="heroicon-o-clipboard-document-list" class="size-4 shrink-0 text-base-content/40" />
                            <span class="min-w-0 flex-1 truncate text-base-content" x-text="row.q?.title ?? ''"></span>
                            <span class="badge badge-sm font-medium" x-text="row.q?.questions_count ?? ''"></span>
                            <x-icon name="heroicon-o-check-circle" class="size-4 shrink-0 text-primary" x-show="Number(selectedQuizId) === row.q?.id" />
                        </div>
                    </div>
                </template>
                <p class="py-6 text-center text-sm text-base-content/50" x-show="!quizzesLoading && quizzes.length > 0 && quizRowsCount === 0">
                    Axtarışa uyğun quiz tapılmadı.
                </p>
            </div>

            <div class="max-h-48 overflow-y-auto px-2 py-2">
                <div class="flex items-center gap-2 px-3 text-xs text-base-content/50" x-show="quizQuestionsLoading">
                    <span class="loading loading-spinner loading-xs text-primary"></span> Suallar yüklənir...
                </div>
                <template x-if="selectedQuizId && !quizQuestionsLoading && quizQuestions.length === 0">
                    <p class="py-8 text-center text-sm text-base-content/50">Bu quizdə sual yoxdur.</p>
                </template>
                <template x-for="q in quizQuestions" :key="q.question_id">
                    <label class="flex cursor-pointer items-start gap-3 rounded-lg px-3 py-2 transition hover:bg-base-200/60">
                        <input type="checkbox" class="checkbox checkbox-primary checkbox-sm mt-0.5 shrink-0" :checked="!!checked[q.question_id]" @change="toggleCheck(q.question_id)" />
                        <span class="min-w-0 flex-1">
                            <span class="rich-preview block text-sm text-base-content" x-html="q.text"></span>
                            <span class="mt-1 flex flex-wrap gap-1">
                                <template x-for="(opt, oi) in itemOptions(q)" :key="oi">
                                    <span class="rounded bg-base-200 px-1.5 py-0.5 text-xs text-base-content/70">
                                        <span class="font-semibold" x-text="opt.letter + '.'"></span> <span x-text="opt.text"></span>
                                    </span>
                                </template>
                            </span>
                        </span>
                    </label>
                </template>
            </div>

            <div class="flex items-center justify-between gap-3 border-t border-base-200 px-5 py-3">
                <span class="text-sm font-medium text-base-content/60" x-text="checkedCount + ' seçildi'"></span>
                <div class="flex items-center gap-2">
                    <button type="button" class="btn btn-sm btn-ghost" @click="showQuizPicker = false">Ləğv et</button>
                    <button type="button" class="btn btn-sm btn-primary" @click="addSelectedFromQuiz()">Əlavə et</button>
                </div>
            </div>
        </div>
    </div>

    {{-- Əl ilə tapşırıq pəncərəsi --}}
    <div x-show="showTaskModal" x-cloak x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-lg overflow-hidden rounded-xl border border-base-300 bg-base-100 shadow-xl">
            <div class="flex items-center justify-between gap-4 border-b border-base-200 px-5 py-4">
                <div>
                    <h3 class="text-base font-semibold text-base-content">Əl ilə tapşırıq yaz</h3>
                    <p class="mt-0.5 text-xs text-base-content/50">Tapşırıq sualı variantsızdır — şagird öz cavabını yazır.</p>
                </div>
                <button type="button" @click="showTaskModal = false" class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200" aria-label="Bağla">
                    <x-icon name="heroicon-o-x-mark" class="size-5" />
                </button>
            </div>
            <div class="px-5 py-4">
                <textarea x-model="taskText" rows="4" placeholder="Tapşırıq mətnini yazın..." class="textarea textarea-bordered w-full text-sm"></textarea>
            </div>
            <div class="flex items-center justify-end gap-2 border-t border-base-200 px-5 py-3">
                <button type="button" class="btn btn-sm btn-ghost" @click="showTaskModal = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="addTask()">Əlavə et</button>
            </div>
        </div>
    </div>
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/homework/controller.js')
@endpush
