@extends('common.layouts.teacher')
@section('title', 'Davam - Kela')
@section('content')
@php
    $attendanceConfig = [
        'workspaces' => $workspaces,
        'workspaceId' => $selectedWorkspaceId,
        'month' => $month,
    ];
@endphp
<div class="space-y-6" x-data="attendanceMonth({{ \Illuminate\Support\Js::from($attendanceConfig) }})">
    <x-teacher.heading subtitle="Workspace seç, ayın günlərində şagirdlərin iştirakını qeyd et">
        Davam
        <x-slot:actions>
            {{-- Avtomatik kayıt göstergesi --}}
            <span class="inline-flex items-center gap-1.5" x-show="saveState !== 'idle'">
                <span class="loading loading-spinner loading-sm text-primary" x-show="saveState === 'saving'"></span>
                <span class="inline-flex items-center gap-1 text-sm font-medium text-success" x-show="saveState === 'saved'">
                    <x-icon name="heroicon-o-check-circle" class="size-4" /> Saxlanıldı
                </span>
            </span>
        </x-slot:actions>
    </x-teacher.heading>

    {{-- Toolbar: workspace + ay --}}
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div class="flex flex-wrap items-center gap-2">
            <select
                x-model="workspaceId"
                @change="loadMonth()"
                class="select select-bordered select-sm text-sm"
            >
                <option value="">Workspace seç...</option>
                <template x-for="ws in workspaces" :key="ws.id">
                    <option :value="ws.id" x-text="ws.name + (ws.student_count > 0 ? ' (' + ws.student_count + ' şagird)' : '')"></option>
                </template>
            </select>

            <div class="inline-flex items-center gap-1">
                <button
                    type="button"
                    class="btn btn-sm btn-ghost border border-base-300"
                    @click="shiftMonth(-1)"
                    title="Əvvəlki ay"
                >
                    <x-icon name="heroicon-o-chevron-left" class="size-4" />
                </button>
                <input
                    type="month"
                    x-model="month"
                    @change="buildDates(); loadMonth()"
                    class="input input-bordered input-sm w-44 text-sm"
                />
                <button
                    type="button"
                    class="btn btn-sm btn-ghost border border-base-300"
                    @click="shiftMonth(1)"
                    title="Növbəti ay"
                >
                    <x-icon name="heroicon-o-chevron-right" class="size-4" />
                </button>
            </div>
        </div>

        {{-- Leqend --}}
        <div class="flex flex-wrap items-center gap-3 text-xs text-base-content/70">
            <span class="inline-flex items-center gap-1"><span class="size-3 rounded bg-success/20"></span> Gəldi</span>
            <span class="inline-flex items-center gap-1"><span class="size-3 rounded bg-error/20"></span> Gəlmədi</span>
            <span class="inline-flex items-center gap-1"><span class="size-3 rounded bg-warning/20"></span> Gecikdi</span>
            <span class="inline-flex items-center gap-1"><span class="size-3 rounded bg-info/20"></span> Üzrlü</span>
            <span class="inline-flex items-center gap-1"><span class="size-3 rounded bg-base-300/40"></span> Qeyd yoxdur</span>
        </div>
    </div>

    {{-- Cədvəl --}}
    <x-teacher.card :padding="false">
        <template x-if="!workspaceId">
            <x-teacher.empty-state icon="clipboard-document-list" title="Workspace seçin" description="Davam cədvəlini açmaq üçün yuxarıdan bir workspace seçin." />
        </template>

        <template x-if="workspaceId && students.length === 0 && !loading">
            <x-teacher.empty-state icon="user-group" title="Şagird yoxdur" description="Bu workspace-ə şagird əlavə olunmayıb." />
        </template>

        <template x-if="workspaceId && students.length > 0">
            <div class="overflow-x-auto">
                {{-- # tarzı kompakt matris: satırlar = şagird, sütunlar = ayın günleri --}}
                <table class="table table-sm w-max">
                    <thead>
                        <tr>
                            <th class="sticky left-0 z-10 min-w-44 bg-base-100 text-xs">Şagird</th>
                            <template x-for="dt in dates" :key="dt.iso">
                                <th
                                    class="h-8 px-0.5 text-center text-[11px] font-semibold"
                                    :class="dt.weekend ? 'bg-base-200/70 text-base-content/40' : 'text-base-content/70'"
                                    :title="dt.iso"
                                >
                                    <span x-text="dt.day"></span>
                                </th>
                            </template>
                        </tr>
                    </thead>
                    <tbody>
                        <template x-for="student in students" :key="student.id">
                            <tr>
                                <td class="sticky left-0 z-10 min-w-44 bg-base-100 text-sm font-medium text-base-content">
                                    <span x-text="student.name"></span>
                                </td>
                                <template x-for="dt in dates" :key="dt.iso">
                                    <td class="p-0.5 text-center">
                                        <button
                                            type="button"
                                            @click="openOptions($event, student.id, dt.iso)"
                                            class="flex size-6 items-center justify-center rounded transition"
                                            :class="cellClass(student.id, dt.iso)"
                                            :title="cellTitle(student.id, dt.iso)"
                                        >
                                            <template x-if="getStatus(student.id, dt.iso) !== 0">
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 1" name="heroicon-o-check" class="size-3" />
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 2" name="heroicon-o-x-mark" class="size-3" />
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 3" name="heroicon-o-clock" class="size-3" />
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 4" name="heroicon-o-exclamation-triangle" class="size-3" />
                                            </template>
                                        </button>
                                    </td>
                                </template>
                            </tr>
                        </template>
                    </tbody>
                </table>
            </div>
        </template>

        <div class="flex items-center justify-between gap-3 border-t border-base-200 px-4 py-2 text-xs text-base-content/60" x-show="workspaceId">
            <span>Bir xanaya klikləyin, status seçin — hər dəyişiklik avtomatik saxlanılır.</span>
            <span class="inline-flex items-center gap-1 font-medium">
                <span class="loading loading-spinner loading-xs text-primary" x-show="saveState === 'saving'"></span>
                <span class="inline-flex items-center gap-1 text-success" x-show="saveState === 'saved'">
                    <x-icon name="heroicon-o-check" class="size-3.5" /> Saxlanıldı
                </span>
            </span>
        </div>
    </x-teacher.card>

    {{-- Status seçim popover --}}
    <template x-if="showOptions">
        <div>
            <div class="fixed inset-0 z-30" @click="showOptions = false"></div>
            <div
                class="fixed z-40 flex items-center gap-1 rounded-xl border border-base-300 bg-base-100 p-1.5 shadow-xl"
                :style="'left: ' + menuLeft + 'px; top: ' + menuTop + 'px; transform: translateX(-50%)'"
                @click.stop
            >
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 0)" class="flex size-8 items-center justify-center rounded-lg text-base-content/40 transition hover:bg-base-200 hover:text-base-content" title="Qeyd yoxdur">
                    <x-icon name="heroicon-o-minus" class="size-4" />
                </button>
                <span class="mx-0.5 h-5 w-px bg-base-200"></span>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 1)" class="flex size-8 items-center justify-center rounded-lg text-success transition hover:bg-success/15" title="Gəldi">
                    <x-icon name="heroicon-o-check" class="size-4" />
                </button>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 2)" class="flex size-8 items-center justify-center rounded-lg text-error transition hover:bg-error/15" title="Gəlmədi">
                    <x-icon name="heroicon-o-x-mark" class="size-4" />
                </button>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 3)" class="flex size-8 items-center justify-center rounded-lg text-warning transition hover:bg-warning/15" title="Gecikdi">
                    <x-icon name="heroicon-o-clock" class="size-4" />
                </button>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 4)" class="flex size-8 items-center justify-center rounded-lg text-info transition hover:bg-info/15" title="Üzrlü">
                    <x-icon name="heroicon-o-exclamation-triangle" class="size-4" />
                </button>
            </div>
        </div>
    </template>
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/attendance/controller.js')
@endpush
