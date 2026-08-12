@extends('common.layouts.teacher')
@section('title', 'Dərslər - Kela')
@section('content')
@php
    $lessonConfig = [
        'folderId' => $folderId > 0 ? $folderId : null,
        'folderTree' => $folderTree,
    ];
@endphp
<div
    class="space-y-6"
    x-data="lessonFolders({{ \Illuminate\Support\Js::from($lessonConfig) }})"
    @keydown.escape.window="closeAll()"
>
    <x-teacher.heading subtitle="Dərsləri qovluqlara bölərək təşkil et">
        Dərslər
        <x-slot:actions>
            <x-teacher.button href="{{ route('teacher.lessons.create', ['folder_id' => $folderId > 0 ? $folderId : null]) }}" icon="plus">Yeni Dərs</x-teacher.button>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Toolbar --}}
    <div class="flex flex-wrap items-center gap-2">
        <button type="button" class="btn btn-sm btn-ghost border border-base-300" @click="openFolderAdd()">
            <x-icon name="heroicon-o-folder-plus" class="size-4" /> Yeni Qovluq
        </button>
    </div>

    {{-- Axtarış + kataloq --}}
    <x-teacher.card :padding="false" @click="onTableClick($event)">
        <form method="GET" action="{{ route('teacher.lessons.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <input type="hidden" name="folder_id" value="{{ $folderId }}" />
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Başlıq ilə axtar..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.lessons.index', ['folder_id' => $folderId > 0 ? $folderId : null]) }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        {{-- Breadcrumb --}}
        <nav class="flex flex-wrap items-center gap-1 border-b border-base-300 px-4 py-2 text-sm">
            <a href="{{ route('teacher.lessons.index') }}" class="inline-flex items-center gap-1 rounded px-2 py-1 font-medium text-primary hover:bg-primary/10">
                <x-icon name="heroicon-o-home" class="size-4" />
                Kök
            </a>
            @foreach ($folders['breadcrumbs'] as $crumb)
                <span class="text-base-content/30">/</span>
                <a href="{{ route('teacher.lessons.index', ['folder_id' => $crumb['id']]) }}" class="rounded px-2 py-1 font-medium text-base-content/70 hover:bg-base-200">
                    {{ $crumb['name'] }}
                </a>
            @endforeach
        </nav>

        @if (count($folders['folders']) === 0 && $lessons->isEmpty())
            <x-teacher.empty-state icon="academic-cap" title="Burada hələ heç nə yoxdur" description="Yeni qovluq açın və ya dərs əlavə edin." />
        @else
            <x-teacher.table :headers="['Ad', 'Tip / Say', 'Video', 'Yayım', 'Sıra', 'Yaradılıb', '']">
                @foreach ($folders['folders'] as $folder)
                    <tr class="transition hover:bg-base-200/50">
                        <td class="font-medium">
                            <a href="{{ route('teacher.lessons.index', ['folder_id' => $folder['id']]) }}" class="inline-flex items-center gap-2 text-primary hover:underline">
                                <x-icon name="heroicon-o-folder" class="size-4 opacity-60" />
                                {{ $folder['name'] }}
                            </a>
                        </td>
                        <td>
                            <x-teacher.badge color="gray">Qovluq · {{ $folder['lesson_count'] }} dərs</x-teacher.badge>
                        </td>
                        <td>—</td>
                        <td>—</td>
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

                @foreach ($lessons as $lesson)
                    <tr class="transition hover:bg-base-200/50">
                        <td class="font-medium text-base-content">
                            <a href="{{ route('teacher.lessons.show', $lesson['content_id']) }}" class="hover:text-primary">{{ $lesson['title'] }}</a>
                            @if ($lesson['description'])
                                <p class="text-xs text-base-content/50">{{ $lesson['description'] }}</p>
                            @endif
                        </td>
                        <td>
                            <x-teacher.badge color="gray">{{ $lesson['has_video'] ? 'Video dərs' : 'Qeyd dərsi' }}</x-teacher.badge>
                        </td>
                        <td>
                            @if ($lesson['has_video'])
                                <x-teacher.badge color="green">
                                    <span class="inline-flex items-center gap-1"><x-icon name="heroicon-o-video-camera" class="size-3" /> {{ $lesson['duration_label'] }}</span>
                                </x-teacher.badge>
                            @else
                                <x-teacher.badge>Yoxdur</x-teacher.badge>
                            @endif
                        </td>
                        <td>
                            <x-teacher.badge :color="$lesson['is_published'] ? 'green' : 'yellow'">
                                {{ $lesson['is_published'] ? 'Yayımlandı' : 'Qaralama' }}
                            </x-teacher.badge>
                        </td>
                        <td class="text-base-content/70">{{ $lesson['order_index'] }}</td>
                        <td class="text-base-content/70">{{ $lesson['created_at'] }}</td>
                        <td class="text-right">
                            <div class="flex items-center justify-end gap-1">
                                <button
                                    data-lesson-action="move"
                                    data-lesson-id="{{ $lesson['content_id'] }}"
                                    data-lesson-title="{{ $lesson['title'] }}"
                                    title="Qovluğa daşı"
                                    class="rounded-lg p-1.5 text-base-content/50 hover:bg-base-200 hover:text-base-content"
                                >
                                    <x-icon name="heroicon-o-arrows-right-left" class="size-4" />
                                </button>
                                <x-teacher.button href="{{ route('teacher.lessons.show', $lesson['content_id']) }}" variant="ghost" size="sm" icon="eye">Aç</x-teacher.button>
                                <x-teacher.button href="{{ route('teacher.lessons.edit', $lesson['content_id']) }}" variant="ghost" size="sm" icon="pencil-square">Redaktə</x-teacher.button>
                                <form
                                    method="POST"
                                    action="{{ route('teacher.lessons.destroy', $lesson['content_id']) }}"
                                    x-data="deleteForm({ url: '/api/v1/lessons/{{ $lesson['content_id'] }}', message: 'Bu dərsi silmək istəyirsiniz?' })"
                                    @submit.prevent="submit"
                                >
                                    @csrf
                                    @method('DELETE')
                                    <x-teacher.button type="submit" variant="ghost" size="sm" icon="trash" x-bind:disabled="busy">Sil</x-teacher.button>
                                </form>
                            </div>
                        </td>
                    </tr>
                @endforeach
            </x-teacher.table>
            <x-teacher.pagination :paginator="$lessons" />
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

    {{-- Dərs daşı dialog --}}
    <div x-show="showLessonMove" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="w-full max-w-md rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 class="mb-4 text-lg font-semibold text-base-content">Dərsi qovluğa daşı</h3>
            <select x-ref="lessonMoveSelect" class="select select-bordered w-full text-sm">
                <option value="">Kökə</option>
                <template x-for="(f, i) in folderTree" :key="f.id">
                    <option :value="f.id" x-text="'&nbsp;'.repeat(f.depth) + f.name"></option>
                </template>
            </select>
            <div class="mt-4 flex justify-end gap-2">
                <button type="button" class="btn btn-sm btn-ghost" @click="showLessonMove = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="saveLessonMove()">Daşı</button>
            </div>
        </div>
    </div>
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/lesson/controller.js')
@endpush
