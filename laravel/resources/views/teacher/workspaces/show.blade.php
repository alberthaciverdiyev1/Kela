@extends('common.layouts.teacher')
@section('title', $workspaceName.' - Kela')
@section('content')
@php
    $workspaceConfig = [
        'workspaceId' => (int) $workspaceId,
        'folderId' => $currentFolderId,
        'folderTree' => $folderTree,
        'availableContents' => $availableContents,
    ];
@endphp
<div
    class="space-y-6"
    x-data="workspaceManager({{ \Illuminate\Support\Js::from($workspaceConfig) }})"
    @keydown.escape.window="closeAll()"
>
    {{-- Header --}}
    <x-teacher.heading :subtitle="count($students).' tələbə'">
        {{ $workspaceName }}
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.workspaces.index') }}" variant="ghost" icon="arrow-left">Geri</x-teacher.button>
            <x-teacher.button href="{{ route('teacher.workspaces.edit', $workspaceId) }}" variant="ghost" icon="pencil-square">Redaktə</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Toolbar --}}
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-2">
            <button type="button" class="btn btn-sm btn-ghost border border-base-300" @click="openFolderAdd()">
                <x-icon name="heroicon-o-folder-plus" class="size-4" /> Yeni Qovluq
            </button>
            <button type="button" class="btn btn-sm btn-ghost border border-base-300" @click="openContentAdd()">
                <x-icon name="heroicon-o-plus-circle" class="size-4" /> Məzmun əlavə et
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

    {{-- Kataloq: breadcrumb + qovluq/content cədvəli --}}
    <x-teacher.card :padding="false" @click="onTableClick($event)">
        {{-- Breadcrumb --}}
        <nav class="flex flex-wrap items-center gap-1 border-b border-base-300 px-4 py-2 text-sm">
            <a href="{{ route('teacher.workspaces.show', $workspaceId) }}" class="inline-flex items-center gap-1 rounded px-2 py-1 font-medium text-primary hover:bg-primary/10">
                <x-icon name="heroicon-o-squares-2x2" class="size-4" />
                {{ $workspaceName }}
            </a>
            @foreach ($directory['breadcrumbs'] as $crumb)
                <span class="text-base-content/30">/</span>
                <a href="{{ route('teacher.workspaces.show', ['workspace' => $workspaceId, 'folder_id' => $crumb['id']]) }}" class="rounded px-2 py-1 font-medium text-base-content/70 hover:bg-base-200">
                    {{ $crumb['name'] }}
                </a>
            @endforeach
        </nav>

        @if (count($directory['folders']) === 0 && count($directory['contents']) === 0)
            <x-teacher.empty-state icon="folder-open" title="Burada hələ heç nə yoxdur" description="Yeni qovluq açın və ya quiz/dərs əlavə edin." />
        @else
            {{-- List görünümü (tablo) --}}
            <div x-show="viewMode === 'list'">
                <x-teacher.table :headers="['Ad', 'Tip', 'Yayım', 'Yaradılıb', '']">
                    @foreach ($directory['folders'] as $folder)
                        <tr class="transition hover:bg-base-200/50">
                            <td class="font-medium">
                                <a href="{{ route('teacher.workspaces.show', ['workspace' => $workspaceId, 'folder_id' => $folder['id']]) }}" class="inline-flex items-center gap-2 text-primary hover:underline">
                                    <x-icon name="heroicon-o-folder" class="size-4 opacity-60" />
                                    {{ $folder['name'] }}
                                </a>
                            </td>
                            <td>
                                <x-teacher.badge color="gray">Qovluq · {{ $folder['content_count'] }} məzmun</x-teacher.badge>
                            </td>
                            <td>—</td>
                            <td>—</td>
                            <td class="text-right">
                                <div class="flex items-center justify-end gap-1">
                                    <button
                                        data-folder-action="rename"
                                        data-folder-id="{{ $folder['id'] }}"
                                        data-folder-name="{{ $folder['name'] }}"
                                        title="Adını dəyiş"
                                        class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
                                    >
                                        <x-icon name="heroicon-o-pencil-square" class="size-4" />
                                    </button>
                                    <button
                                        data-folder-action="move"
                                        data-folder-id="{{ $folder['id'] }}"
                                        title="Daşı"
                                        class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
                                    >
                                        <x-icon name="heroicon-o-arrows-right-left" class="size-4" />
                                    </button>
                                    <button
                                        data-folder-action="remove"
                                        data-folder-id="{{ $folder['id'] }}"
                                        data-folder-name="{{ $folder['name'] }}"
                                        title="Workspace-dən çıxar"
                                        class="rounded-lg p-1.5 text-base-content/50 hover:bg-warning/10 hover:text-warning"
                                    >
                                        <x-icon name="heroicon-o-arrow-up-tray" class="size-4" />
                                    </button>
                                    <button
                                        data-folder-action="delete"
                                        data-folder-id="{{ $folder['id'] }}"
                                        data-folder-name="{{ $folder['name'] }}"
                                        title="Sil"
                                        class="rounded-lg p-1.5 text-error/70 hover:bg-error/10 hover:text-error"
                                    >
                                        <x-icon name="heroicon-o-trash" class="size-4" />
                                    </button>
                                </div>
                            </td>
                        </tr>
                    @endforeach

                    @foreach ($directory['contents'] as $item)
                        <tr class="transition hover:bg-base-200/50">
                            <td class="font-medium text-base-content">
                                @if ($item['type'] === \App\Domain\Content\Content::TYPE_QUIZ)
                                    <a href="{{ route('teacher.quizzes.edit', $item['content_id']) }}" class="inline-flex items-center gap-2 hover:text-primary">
                                        <x-icon name="heroicon-o-clipboard-document-list" class="size-4 opacity-60" />
                                        {{ $item['title'] }}
                                    </a>
                                @else
                                    <a href="{{ route('teacher.lessons.edit', $item['content_id']) }}" class="inline-flex items-center gap-2 hover:text-primary">
                                        <x-icon name="heroicon-o-academic-cap" class="size-4 opacity-60" />
                                        {{ $item['title'] }}
                                    </a>
                                @endif
                            </td>
                            <td>
                                <x-teacher.badge :color="$item['type'] === \App\Domain\Content\Content::TYPE_QUIZ ? 'blue' : 'green'">{{ $item['type_label'] }}</x-teacher.badge>
                            </td>
                            <td>
                                <x-teacher.badge :color="$item['is_published'] ? 'green' : 'yellow'">
                                    {{ $item['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                                </x-teacher.badge>
                            </td>
                            <td class="text-base-content/70">{{ $item['created_at'] }}</td>
                            <td class="text-right">
                                <div class="flex items-center justify-end gap-1">
                                    <button
                                        data-content-action="move"
                                        data-content-id="{{ $item['content_id'] }}"
                                        data-content-title="{{ $item['title'] }}"
                                        title="Qovluğa daşı"
                                        class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
                                    >
                                        <x-icon name="heroicon-o-arrows-right-left" class="size-4" />
                                    </button>
                                    <button
                                        data-content-action="remove"
                                        data-content-id="{{ $item['content_id'] }}"
                                        data-content-title="{{ $item['title'] }}"
                                        title="Workspace-dən çıxar"
                                        class="rounded-lg p-1.5 text-base-content/50 hover:bg-warning/10 hover:text-warning"
                                    >
                                        <x-icon name="heroicon-o-arrow-up-tray" class="size-4" />
                                    </button>
                                    @if ($item['type'] === \App\Domain\Content\Content::TYPE_QUIZ)
                                        <x-teacher.button href="{{ route('teacher.quizzes.edit', $item['content_id']) }}" variant="ghost" size="sm" icon="pencil-square">Redaktə</x-teacher.button>
                                    @else
                                        <x-teacher.button href="{{ route('teacher.lessons.edit', $item['content_id']) }}" variant="ghost" size="sm" icon="pencil-square">Redaktə</x-teacher.button>
                                    @endif
                                </div>
                            </td>
                        </tr>
                    @endforeach
                </x-teacher.table>
            </div>

            {{-- Grid görünümü (dikey kartlar — 6 sütun, kompakt, böyük ikon) --}}
            <div x-show="viewMode === 'grid'" class="grid grid-cols-6 gap-2 p-3">
                @foreach ($directory['folders'] as $folder)
                    <div class="group relative flex flex-col items-center rounded-lg border border-base-300 bg-base-100 p-2 text-center transition hover:border-primary/40 hover:shadow-sm">
                        <a href="{{ route('teacher.workspaces.show', ['workspace' => $workspaceId, 'folder_id' => $folder['id']]) }}" class="relative flex size-20 items-center justify-center rounded-xl bg-primary/10 text-primary transition group-hover:bg-primary/15">
                            <x-icon name="heroicon-o-folder" class="size-[66px]" />
                            @if ($folder['content_count'] > 0)
                                <span class="absolute -right-2 -top-2 flex size-5 items-center justify-center rounded-full bg-primary text-[10px] font-bold leading-none text-primary-content">
                                    {{ $folder['content_count'] }}
                                </span>
                            @endif
                        </a>
                        <a href="{{ route('teacher.workspaces.show', ['workspace' => $workspaceId, 'folder_id' => $folder['id']]) }}" class="mt-1.5 block w-full truncate text-xs font-medium leading-tight text-base-content transition hover:text-primary" title="{{ $folder['name'] }}">
                            {{ $folder['name'] }}
                        </a>
                        <div class="mt-1 flex items-center justify-center gap-0.5 opacity-0 transition-opacity duration-150 group-hover:opacity-100">
                            <button
                                data-folder-action="rename"
                                data-folder-id="{{ $folder['id'] }}"
                                data-folder-name="{{ $folder['name'] }}"
                                title="Adını dəyiş"
                                class="rounded-md p-0.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
                            >
                                <x-icon name="heroicon-o-pencil-square" class="size-3" />
                            </button>
                            <button
                                data-folder-action="move"
                                data-folder-id="{{ $folder['id'] }}"
                                title="Daşı"
                                class="rounded-md p-0.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
                            >
                                <x-icon name="heroicon-o-arrows-right-left" class="size-3" />
                            </button>
                            <button
                                data-folder-action="remove"
                                data-folder-id="{{ $folder['id'] }}"
                                data-folder-name="{{ $folder['name'] }}"
                                title="Workspace-dən çıxar"
                                class="rounded-md p-0.5 text-base-content/50 hover:bg-warning/10 hover:text-warning"
                            >
                                <x-icon name="heroicon-o-arrow-up-tray" class="size-3" />
                            </button>
                            <button
                                data-folder-action="delete"
                                data-folder-id="{{ $folder['id'] }}"
                                data-folder-name="{{ $folder['name'] }}"
                                title="Sil"
                                class="rounded-md p-0.5 text-error/70 hover:bg-error/10 hover:text-error"
                            >
                                <x-icon name="heroicon-o-trash" class="size-3" />
                            </button>
                        </div>
                    </div>
                @endforeach

                @foreach ($directory['contents'] as $item)
                    <div class="group relative flex flex-col items-center rounded-lg border border-base-300 bg-base-100 p-2 text-center transition hover:border-primary/40 hover:shadow-sm">
                        @if ($item['type'] === \App\Domain\Content\Content::TYPE_QUIZ)
                            <a href="{{ route('teacher.quizzes.edit', $item['content_id']) }}" class="relative flex size-20 items-center justify-center rounded-xl transition {{ $item['is_published'] ? 'bg-info/10 text-info group-hover:bg-info/15' : 'bg-warning/10 text-warning group-hover:bg-warning/15' }}">
                                <x-icon name="heroicon-o-clipboard-document-list" class="size-[66px]" />
                            </a>
                        @else
                            <a href="{{ route('teacher.lessons.edit', $item['content_id']) }}" class="relative flex size-20 items-center justify-center rounded-xl transition {{ $item['is_published'] ? 'bg-success/10 text-success group-hover:bg-success/15' : 'bg-warning/10 text-warning group-hover:bg-warning/15' }}">
                                <x-icon name="heroicon-o-video-camera" class="size-[66px]" />
                            </a>
                        @endif
                        @if ($item['type'] === \App\Domain\Content\Content::TYPE_QUIZ)
                            <a href="{{ route('teacher.quizzes.edit', $item['content_id']) }}" class="mt-1.5 block w-full truncate text-xs font-medium leading-tight text-base-content transition hover:text-primary" title="{{ $item['title'] }}">
                                {{ $item['title'] }}
                            </a>
                        @else
                            <a href="{{ route('teacher.lessons.edit', $item['content_id']) }}" class="mt-1.5 block w-full truncate text-xs font-medium leading-tight text-base-content transition hover:text-primary" title="{{ $item['title'] }}">
                                {{ $item['title'] }}
                            </a>
                        @endif
                        <div class="mt-1 flex items-center justify-center gap-0.5 opacity-0 transition-opacity duration-150 group-hover:opacity-100">
                            <button
                                data-content-action="move"
                                data-content-id="{{ $item['content_id'] }}"
                                data-content-title="{{ $item['title'] }}"
                                title="Qovluğa daşı"
                                class="rounded-md p-0.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
                            >
                                <x-icon name="heroicon-o-arrows-right-left" class="size-3" />
                            </button>
                            <button
                                data-content-action="remove"
                                data-content-id="{{ $item['content_id'] }}"
                                data-content-title="{{ $item['title'] }}"
                                title="Workspace-dən çıxar"
                                class="rounded-md p-0.5 text-base-content/50 hover:bg-warning/10 hover:text-warning"
                            >
                                <x-icon name="heroicon-o-arrow-up-tray" class="size-3" />
                            </button>
                            @if ($item['type'] === \App\Domain\Content\Content::TYPE_QUIZ)
                                <a href="{{ route('teacher.quizzes.edit', $item['content_id']) }}" class="rounded-md p-0.5 text-base-content/50 hover:bg-base-200 hover:text-base-content" title="Redaktə">
                                    <x-icon name="heroicon-o-pencil-square" class="size-3" />
                                </a>
                            @else
                                <a href="{{ route('teacher.lessons.edit', $item['content_id']) }}" class="rounded-md p-0.5 text-base-content/50 hover:bg-base-200 hover:text-base-content" title="Redaktə">
                                    <x-icon name="heroicon-o-pencil-square" class="size-3" />
                                </a>
                            @endif
                        </div>
                    </div>
                @endforeach
            </div>
        @endif
    </x-teacher.card>

    {{-- Tələbələr --}}
    <div class="space-y-3">
        <div class="flex items-center justify-between">
            <h3 class="flex items-center gap-2 text-lg font-semibold text-base-content">
                Tələbələr
                <x-teacher.badge color="blue">{{ count($students) }}</x-teacher.badge>
            </h3>
            <button type="button" class="btn btn-sm btn-ghost border border-base-300" @click="showStudent = true">
                <x-icon name="heroicon-o-user-plus" class="size-4" /> Tələbə Əlavə Et
            </button>
        </div>

        @if (count($students) === 0)
            <x-teacher.empty-state icon="user-group" title="Tələbə yoxdur" description="Bu workspace-ə tələbə əlavə edin." />
        @else
            <x-teacher.card :padding="false">
                <x-teacher.table :headers="['Ad', 'E-poçt', '']">
                    @foreach ($students as $student)
                        <tr>
                            <td class="font-medium text-base-content">{{ $student['name'] }}</td>
                            <td class="text-base-content/70">{{ $student['email'] }}</td>
                            <td class="text-right">
                                <button
                                    data-student-id="{{ $student['id'] }}"
                                    data-student-name="{{ $student['name'] }}"
                                    title="Çıxar"
                                    @click="detachStudent($event.currentTarget)"
                                    class="rounded-lg p-1.5 text-error/70 hover:bg-error/10 hover:text-error"
                                >
                                    <x-icon name="heroicon-o-user-minus" class="size-4" />
                                </button>
                            </td>
                        </tr>
                    @endforeach
                </x-teacher.table>
            </x-teacher.card>
        @endif
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

    {{-- Məzmun daşı dialog --}}
    <div x-show="showContentMove" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Məzmunu qovluğa daşı</h3>
            <select x-ref="contentMoveSelect" class="select select-bordered w-full text-sm">
                <option value="">Bu workspace-in kökünə</option>
                <template x-for="(f, i) in folderTree" :key="f.id">
                    <option :value="f.id" x-text="'&nbsp;'.repeat(f.depth) + f.name"></option>
                </template>
            </select>
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showContentMove = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveContentMove()">Daşı</button>
            </div>
        </div>
    </div>

    {{-- Məzmun əlavə et dialog --}}
    <div x-show="showContentAdd" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="flex w-full max-w-2xl flex-col overflow-hidden rounded-xl bg-base-100 shadow-2xl">
            {{-- Başlıq --}}
            <div class="flex items-center justify-between gap-4 px-6 py-4">
                <div>
                    <h3 class="text-lg font-semibold text-base-content">Məzmun əlavə et</h3>
                    <p class="mt-0.5 text-xs text-base-content/50">Qovluğu bütöv (içindəki məzmunlarla) və ya məzmunları ayrıca seçə bilərsiniz.</p>
                </div>
                <button
                    type="button"
                    @click="showContentAdd = false"
                    class="rounded-lg p-1.5 text-base-content/50 transition hover:bg-base-200 hover:text-base-content"
                    aria-label="Bağla"
                >
                    <x-icon name="heroicon-o-x-mark" class="size-5" />
                </button>
            </div>

            {{-- Axtarış + filtre + hədəf --}}
            <div class="flex flex-wrap items-center gap-2 border-y border-base-200 bg-base-50 px-6 py-3">
                <div class="relative min-w-0 flex-1">
                    <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                        <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                    </span>
                    <input
                        type="text"
                        x-model="contentSearch"
                        placeholder="Axtar..."
                        class="input input-bordered w-full pl-9 text-sm"
                    />
                </div>
                <select
                    x-model="contentTypeFilter"
                    class="select select-bordered text-sm"
                    x-ref="contentTypeSelect"
                >
                    <option value="">Bütün növlər</option>
                    <option value="1">Quiz</option>
                    <option value="0">Dərs</option>
                </select>
                <div class="flex items-center gap-2">
                    <span class="text-xs font-medium text-base-content/60">Haraya:</span>
                    <select x-ref="contentAddSelect" class="select select-bordered text-sm">
                        <option value="">Kök</option>
                        <template x-for="(f, i) in folderTree" :key="f.id">
                            <option :value="f.id" x-text="'&nbsp;'.repeat(f.depth * 2) + f.name"></option>
                        </template>
                    </select>
                </div>
            </div>

            {{-- Liste: qovluqlar + içindəki məzmunlar (hamısı açıq) --}}
            <div class="max-h-[340px] overflow-y-auto px-4 py-2">
                <template x-if="availableContents.length === 0">
                    <p class="py-10 text-center text-sm text-base-content/50">
                        Əlavə edilə bilən məzmun yoxdur.
                    </p>
                </template>

                <template x-if="visibleContentCount === 0 && availableContents.length > 0">
                    <p class="py-10 text-center text-sm text-base-content/50">
                        Axtarışa uyğun məzmun tapılmadı.
                    </p>
                </template>

                <template x-for="(row, i) in visibleContentRows" :key="row.key">
                    <div class="space-y-0.5">
                        {{-- Qovluq başlığı (bütöv əlavə edilə bilər) --}}
                        <label
                            x-show="row.kind === 'folder'"
                            class="flex cursor-pointer items-center gap-2 pt-3 pb-1 text-xs font-semibold uppercase tracking-wide text-base-content/50"
                            :style="'padding-left: ' + (row.depth * 16 + 4) + 'px'"
                        >
                            <input
                                type="checkbox"
                                class="checkbox checkbox-primary checkbox-sm shrink-0"
                                :checked="isFolderSelected(row) || isFolderCovered(row)"
                                @change="toggleFolderSelection(row)"
                                :disabled="row.count === 0 || row.folder_id == null || isFolderCovered(row)"
                            />
                            <x-icon name="heroicon-o-folder" class="size-4 text-primary/70" />
                            <span class="min-w-0 flex-1 truncate" x-text="row.name"></span>
                            <span class="text-base-content/40" x-text="row.count"></span>
                        </label>

                        {{-- Məzmun sətri --}}
                        <label
                            x-show="row.kind === 'content'"
                            class="flex cursor-pointer select-none items-center gap-3 rounded-lg px-3 py-2 text-sm transition hover:bg-base-200/60"
                            :class="isContentSelected(row.c) ? 'bg-primary/5' : ''"
                            :style="'padding-left: ' + (row.depth * 16 + 12) + 'px'"
                        >
                            <input
                                type="checkbox"
                                :value="row.c.content_id"
                                class="checkbox checkbox-primary checkbox-sm shrink-0"
                                @change="updateContentSelection($event)"
                                :checked="isContentSelected(row.c)"
                                :disabled="isContentCoveredByFolder(row.c)"
                            />
                            <span class="min-w-0 flex-1 truncate text-base-content" x-text="row.c.title"></span>
                            <span
                                class="badge badge-sm font-medium"
                                :class="row.c.type === 1 ? 'badge-info' : 'badge-success'"
                                x-text="row.c.type_label"
                            ></span>
                        </label>
                    </div>
                </template>
            </div>

            {{-- Alt bilgi --}}
            <div class="flex items-center justify-between gap-3 border-t border-base-200 bg-base-50 px-6 py-3">
                <span
                    class="text-sm font-medium"
                    :class="selectedContentCount > 0 ? 'text-primary' : 'text-base-content/50'"
                    x-text="selectionSummary"
                ></span>
                <div class="flex items-center gap-2">
                    <button type="button" class="btn btn-sm btn-ghost" @click="showContentAdd = false">Ləğv et</button>
                    <button
                        type="button"
                        class="btn btn-sm btn-primary"
                        @click="saveContentAdd()"
                        x-bind:disabled="selectedContentCount === 0"
                    >
                        Əlavə et
                    </button>
                </div>
            </div>
        </div>
    </div>

    {{-- Tələbə dialog --}}
    <div x-show="showStudent" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Tələbə Əlavə Et</h3>
            <select x-ref="studentSelect" multiple class="select select-bordered h-40 w-full text-sm">
                @foreach ($availableStudents as $value => $label)
                    <option value="{{ $value }}">{{ $label }}</option>
                @endforeach
            </select>
            @if (count($availableStudents) === 0)
                <p class="mt-2 text-sm text-base-content/60">Əlavə edilə bilən tələbə yoxdur.</p>
            @endif
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showStudent = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveStudents()">Əlavə Et</button>
            </div>
        </div>
    </div>
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/workspace/controller.js')
@endpush
