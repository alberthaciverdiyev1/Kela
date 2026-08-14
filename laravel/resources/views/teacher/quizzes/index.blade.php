@extends('common.layouts.teacher')
@section('title', 'Quizlər - Kela')
@section('content')
@php
    $quizConfig = [
        'folderId' => $folderId > 0 ? $folderId : null,
        'folderTree' => $folderTree,
    ];
@endphp
<div
    class="space-y-6"
    x-data="quizFolders({{ \Illuminate\Support\Js::from($quizConfig) }})"
    @keydown.escape.window="closeAll()"
>
    <x-teacher.heading subtitle="Quizləri qovluqlara bölərək təşkil et">
        Quizlər
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.quizzes.create', ['folder_id' => $folderId > 0 ? $folderId : null]) }}" icon="plus">Yeni Quiz</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Toolbar --}}
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-2">
            <button type="button" class="btn btn-sm btn-ghost border border-base-300" @click="openFolderAdd()">
                <x-icon name="heroicon-o-folder-plus" class="size-4" /> Yeni Qovluq
            </button>
        </div>

        {{-- Görünüm keçidi: List / Grid --}}
        <div class="inline-flex items-center rounded-lg border border-base-300 bg-base-100 p-0.5">
            <button
                type="button"
                @click="setViewMode('list')"
                class="inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition"
                :class="viewMode === 'list' ? 'bg-primary/10 text-primary' : 'text-base-content/60 hover:text-base-content'"
                title="Liste görünümü"
            >
                <x-icon name="heroicon-o-list-bullet" class="size-4" /> List
            </button>
            <button
                type="button"
                @click="setViewMode('grid')"
                class="inline-flex items-center gap-1.5 rounded-md px-3 py-1.5 text-sm font-medium transition"
                :class="viewMode === 'grid' ? 'bg-primary/10 text-primary' : 'text-base-content/60 hover:text-base-content'"
                title="Grid görünümü"
            >
                <x-icon name="heroicon-o-squares-2x2" class="size-4" /> Grid
            </button>
        </div>
    </div>

    {{-- Axtarış + kataloq --}}
    <x-teacher.card :padding="false">
        <form method="GET" action="{{ route('teacher.quizzes.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <input type="hidden" name="folder_id" value="{{ $folderId }}" />
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Başlıq ilə axtar..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.quizzes.index', ['folder_id' => $folderId > 0 ? $folderId : null]) }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        {{-- Breadcrumb --}}
        <nav class="flex flex-wrap items-center gap-1 border-b border-base-300 px-4 py-2 text-sm">
            <a href="{{ route('teacher.quizzes.index') }}" class="inline-flex items-center gap-1 rounded px-2 py-1 font-medium text-primary hover:bg-primary/10">
                <x-icon name="heroicon-o-home" class="size-4" />
                Kök
            </a>
            @foreach ($folders['breadcrumbs'] as $crumb)
                <span class="text-base-content/30">/</span>
                <a href="{{ route('teacher.quizzes.index', ['folder_id' => $crumb['id']]) }}" class="rounded px-2 py-1 font-medium text-base-content/70 hover:bg-base-200">
                    {{ $crumb['name'] }}
                </a>
            @endforeach
        </nav>

        @if (count($folders['folders']) === 0 && $quizzes->isEmpty())
            <x-teacher.empty-state icon="clipboard-document-list" title="Burada hələ heç nə yoxdur" description="Yeni qovluq açın və ya quiz əlavə edin." />
        @else
            {{-- List görünümü (tablo) --}}
            <div x-show="viewMode === 'list'">
            <x-teacher.table :headers="['Ad', 'Tip / Say', 'Yayım', 'Yaradılıb']">
                @foreach ($folders['folders'] as $folder)
                    <tr
                        class="cursor-context-menu transition hover:bg-base-200/50"
                        data-kind="folder"
                        data-folder-id="{{ $folder['id'] }}"
                        data-folder-name="{{ $folder['name'] }}"
                        @contextmenu.prevent="openRowContextMenu($event, 'folder', $el)"
                    >
                        <td class="font-medium">
                            <a href="{{ route('teacher.quizzes.index', ['folder_id' => $folder['id']]) }}" class="inline-flex items-center gap-2 text-primary hover:underline">
                                <x-icon name="heroicon-o-folder" class="size-4 opacity-60" />
                                {{ $folder['name'] }}
                            </a>
                        </td>
                        <td>
                            <x-teacher.badge color="gray">Qovluq · {{ $folder['quiz_count'] }} quiz</x-teacher.badge>
                        </td>
                        <td>—</td>
                        <td>—</td>
                    </tr>
                @endforeach

                @foreach ($quizzes as $quiz)
                    <tr
                        class="cursor-context-menu transition hover:bg-base-200/50"
                        data-kind="quiz"
                        data-quiz-id="{{ $quiz['content_id'] }}"
                        data-quiz-title="{{ $quiz['title'] }}"
                        data-open-url="{{ route('teacher.quizzes.show', $quiz['content_id']) }}"
                        data-edit-url="{{ route('teacher.quizzes.edit', $quiz['content_id']) }}"
                        @contextmenu.prevent="openRowContextMenu($event, 'quiz', $el)"
                    >
                        <td class="font-medium text-base-content">
                            <a href="{{ route('teacher.quizzes.show', $quiz['content_id']) }}" class="hover:text-primary">{{ $quiz['title'] }}</a>
                            @if ($quiz['description'])
                                <p class="text-xs text-base-content/50">{{ $quiz['description'] }}</p>
                            @endif
                        </td>
                        <td>
                            <x-teacher.badge color="gray">{{ $quiz['questions_count'] }} sual</x-teacher.badge>
                        </td>
                        <td>
                            <x-teacher.badge :color="$quiz['is_published'] ? 'green' : 'yellow'">
                                {{ $quiz['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                            </x-teacher.badge>
                        </td>
                        <td class="text-base-content/70">{{ $quiz['created_at'] }}</td>
                    </tr>
                @endforeach
            </x-teacher.table>
            </div>

            {{-- Grid görünümü (dikey kartlar — 6 sütun, kompakt, böyük ikon) --}}
            <div x-show="viewMode === 'grid'" class="grid grid-cols-6 gap-2 p-3">
                @foreach ($folders['folders'] as $folder)
                    <div
                        class="group relative flex cursor-context-menu flex-col items-center rounded-lg border border-base-300 bg-base-100 p-2 text-center transition hover:border-primary/40 hover:shadow-sm"
                        data-kind="folder"
                        data-folder-id="{{ $folder['id'] }}"
                        data-folder-name="{{ $folder['name'] }}"
                        @contextmenu.prevent="openRowContextMenu($event, 'folder', $el)"
                    >
                        <a href="{{ route('teacher.quizzes.index', ['folder_id' => $folder['id']]) }}" class="relative flex size-20 items-center justify-center rounded-xl bg-primary/10 text-primary transition group-hover:bg-primary/15">
                            <x-icon name="heroicon-o-folder" class="size-[66px]" />
                            @if ($folder['quiz_count'] > 0)
                                <span class="absolute -right-2 -top-2 flex size-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold leading-none text-primary-content">
                                    {{ $folder['quiz_count'] }}
                                </span>
                            @endif
                        </a>
                        <a href="{{ route('teacher.quizzes.index', ['folder_id' => $folder['id']]) }}" class="mt-1.5 block w-full truncate text-xs font-medium leading-tight text-base-content transition hover:text-primary" title="{{ $folder['name'] }}">
                            {{ $folder['name'] }}
                        </a>
                    </div>
                @endforeach

                @foreach ($quizzes as $quiz)
                    <div
                        class="group relative flex cursor-context-menu flex-col items-center rounded-lg border border-base-300 bg-base-100 p-2 text-center transition hover:border-primary/40 hover:shadow-sm"
                        data-kind="quiz"
                        data-quiz-id="{{ $quiz['content_id'] }}"
                        data-quiz-title="{{ $quiz['title'] }}"
                        data-open-url="{{ route('teacher.quizzes.show', $quiz['content_id']) }}"
                        data-edit-url="{{ route('teacher.quizzes.edit', $quiz['content_id']) }}"
                        @contextmenu.prevent="openRowContextMenu($event, 'quiz', $el)"
                    >
                        <a href="{{ route('teacher.quizzes.show', $quiz['content_id']) }}" class="relative flex size-20 items-center justify-center rounded-xl transition {{ $quiz['is_published'] ? 'bg-info/10 text-info group-hover:bg-info/15' : 'bg-warning/10 text-warning group-hover:bg-warning/15' }}">
                            <x-icon name="heroicon-o-clipboard-document-list" class="size-[66px]" />
                        </a>
                        <a href="{{ route('teacher.quizzes.show', $quiz['content_id']) }}" class="mt-1.5 block w-full truncate text-xs font-medium leading-tight text-base-content transition hover:text-primary" title="{{ $quiz['title'] }}">
                            {{ $quiz['title'] }}
                        </a>
                    </div>
                @endforeach
            </div>
            <x-teacher.pagination :paginator="$quizzes" />
        @endif
    </x-teacher.card>

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

    {{-- Quiz daşı dialog --}}
    <div x-show="showQuizMove" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Quiz-i qovluğa daşı</h3>
            <select x-ref="quizMoveSelect" class="select select-bordered w-full text-sm">
                <option value="">Kökə</option>
                <template x-for="(f, i) in folderTree" :key="f.id">
                    <option :value="f.id" x-text="'&nbsp;'.repeat(f.depth) + f.name"></option>
                </template>
            </select>
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showQuizMove = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveQuizMove()">Daşı</button>
            </div>
        </div>
    </div>

    {{-- Sağ-tık kontekst menyusu --}}
    @include('teacher.partials._context-menu')
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/quiz/controller.js')
@endpush
