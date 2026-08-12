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
        <div class="inline-flex flex-wrap items-center gap-x-4 gap-y-2 rounded-lg border border-base-200 bg-base-100 px-3 py-1.5 text-xs font-medium text-base-content/70">
            <span class="inline-flex items-center gap-1.5"><span class="flex size-4 items-center justify-center rounded bg-success text-success-content"><x-icon name="heroicon-o-check" class="size-2.5" /></span> Gəldi</span>
            <span class="inline-flex items-center gap-1.5"><span class="flex size-4 items-center justify-center rounded bg-error text-error-content"><x-icon name="heroicon-o-x-mark" class="size-2.5" /></span> Gəlmədi</span>
            <span class="inline-flex items-center gap-1.5"><span class="flex size-4 items-center justify-center rounded bg-warning text-warning-content"><x-icon name="heroicon-o-clock" class="size-2.5" /></span> Gecikdi</span>
            <span class="inline-flex items-center gap-1.5"><span class="flex size-4 items-center justify-center rounded bg-info text-info-content"><x-icon name="heroicon-o-exclamation-triangle" class="size-2.5" /></span> Üzrlü</span>
            <span class="inline-flex items-center gap-1.5 text-base-content/50"><span class="size-4 rounded border border-dashed border-base-content/30"></span> Qeyd yoxdur</span>
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
            <div class="overflow-hidden rounded-lg border border-base-300">
                <div class="overflow-x-auto">
                <table class="table w-full border-collapse">
                    <thead>
                        <tr>
                            <th class="sticky left-0 z-20 w-48 border border-base-200 bg-base-100 px-4 text-left text-[15px] font-semibold text-base-content/70">Şagird</th>
                            <template x-for="dt in dates" :key="dt.iso">
                                <th
                                    class="border border-base-200 px-1 py-2 text-center"
                                    :class="dt.weekend ? 'bg-error/10' : ''"
                                    :title="dt.iso"
                                >
                                    <div class="flex flex-col items-center leading-none">
                                        <span class="text-[10px] font-semibold uppercase tracking-wider" :class="dt.weekend ? 'text-base-content/35' : 'text-base-content/45'">
                                            <span x-text="dt.weekdayLabel"></span>
                                        </span>
                                        <span
                                            class="mt-1 flex size-6 items-center justify-center rounded-full text-xs font-bold"
                                            :class="dt.isToday ? 'bg-primary text-primary-content' : (dt.weekend ? 'text-base-content/40' : 'text-base-content/80')"
                                        >
                                            <span x-text="dt.day"></span>
                                        </span>
                                    </div>
                                </th>
                            </template>
                            <th class="w-40 border border-base-200 bg-base-200/40 px-2 text-center text-xs font-semibold text-base-content/70">Cəm</th>
                        </tr>
                    </thead>
                    <tbody>
                        <template x-for="student in students" :key="student.id">
                            <tr class="group transition hover:bg-base-200/30">
                                <td class="sticky left-0 z-20 w-48 border border-base-200 bg-base-100 px-4 text-[15px] font-medium text-base-content">
                                    <span x-text="student.name"></span>
                                </td>
                                <template x-for="dt in dates" :key="dt.iso">
                                    <td
                                        class="min-w-10 cursor-pointer border border-base-200 p-0 text-center transition"
                                        :class="dt.isToday ? 'bg-primary/5' : (dt.weekend ? 'bg-error/10' : '')"
                                        @click="openOptions($event, student.id, dt.iso)"
                                        :title="cellTitle(student.id, dt.iso)"
                                    >
                                        <button
                                            type="button"
                                            class="flex h-10 w-full items-center justify-center transition"
                                            :class="cellClass(student.id, dt.iso)"
                                        >
                                            <template x-if="getStatus(student.id, dt.iso) !== 0">
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 1" name="heroicon-o-check" class="size-4" />
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 2" name="heroicon-o-x-mark" class="size-4" />
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 3" name="heroicon-o-clock" class="size-4" />
                                                <x-icon x-show="getStatus(student.id, dt.iso) === 4" name="heroicon-o-exclamation-triangle" class="size-4" />
                                            </template>
                                        </button>
                                    </td>
                                </template>
                                <td class="w-40 border border-base-200 bg-base-200/40 px-2">
                                    <div class="flex items-center justify-center gap-1" x-show="studentSummary(student.id).total > 0">
                                        <span class="inline-flex items-center gap-0.5 rounded-md bg-success/15 px-1.5 py-0.5 text-xs font-bold text-success" x-show="studentSummary(student.id).present > 0">
                                            <x-icon name="heroicon-o-check" class="size-3.5" /><span x-text="studentSummary(student.id).present"></span>
                                        </span>
                                        <span class="inline-flex items-center gap-0.5 rounded-md bg-error/15 px-1.5 py-0.5 text-xs font-bold text-error" x-show="studentSummary(student.id).absent > 0">
                                            <x-icon name="heroicon-o-x-mark" class="size-3.5" /><span x-text="studentSummary(student.id).absent"></span>
                                        </span>
                                        <span class="inline-flex items-center gap-0.5 rounded-md bg-warning/15 px-1.5 py-0.5 text-xs font-bold text-warning" x-show="studentSummary(student.id).late > 0">
                                            <x-icon name="heroicon-o-clock" class="size-3.5" /><span x-text="studentSummary(student.id).late"></span>
                                        </span>
                                        <span class="inline-flex items-center gap-0.5 rounded-md bg-info/15 px-1.5 py-0.5 text-xs font-bold text-info" x-show="studentSummary(student.id).excused > 0">
                                            <x-icon name="heroicon-o-exclamation-triangle" class="size-3.5" /><span x-text="studentSummary(student.id).excused"></span>
                                        </span>
                                    </div>
                                    <div class="flex items-center justify-center text-base-content/30" x-show="studentSummary(student.id).total === 0">
                                        <span>—</span>
                                    </div>
                                </td>
                            </tr>
                        </template>
                    </tbody>
                </table>
                </div>
            </div>
        </template>

        <div class="flex items-center justify-between gap-3 border-t border-base-200 px-4 py-2 text-xs text-base-content/60" x-show="workspaceId">
            <span class="inline-flex items-center gap-1.5">
                <x-icon name="heroicon-o-information-circle" class="size-3.5" />
                Bir xanaya klikləyin, status seçin — hər dəyişiklik avtomatik saxlanılır.
            </span>
            <span class="inline-flex items-center gap-1 font-medium">
                <span class="loading loading-spinner loading-xs text-primary" x-show="saveState === 'saving'"></span>
                <span class="inline-flex items-center gap-1 text-success" x-show="saveState === 'saved'">
                    <x-icon name="heroicon-o-check-circle" class="size-3.5" /> Saxlanıldı
                </span>
            </span>
        </div>
    </x-teacher.card>

    {{-- Status seçim popoverı (etiketli şaquli menyu) --}}
    <template x-if="showOptions">
        <div>
            <div class="fixed inset-0 z-30" @click="showOptions = false"></div>
            <div
                class="fixed z-40 w-48 overflow-hidden rounded-xl border border-base-300 bg-base-100 py-1 shadow-xl"
                :style="'left: ' + menuLeft + 'px; top: ' + menuTop + 'px; transform: translateX(-50%)'"
                @click.stop
            >
                <p class="border-b border-base-200 px-3 py-2 text-[11px] font-semibold uppercase tracking-wide text-base-content/50" x-text="popoverTitle"></p>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 1)" class="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium transition hover:bg-success/10 hover:text-success">
                    <span class="flex size-6 items-center justify-center rounded-md bg-success text-success-content"><x-icon name="heroicon-o-check" class="size-3.5" /></span>
                    <span class="flex-1">Gəldi</span>
                    <x-icon name="heroicon-o-check" class="size-4 text-success" x-show="getStatus(activeCell.studentId, activeCell.iso) === 1" />
                </button>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 2)" class="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium transition hover:bg-error/10 hover:text-error">
                    <span class="flex size-6 items-center justify-center rounded-md bg-error text-error-content"><x-icon name="heroicon-o-x-mark" class="size-3.5" /></span>
                    <span class="flex-1">Gəlmədi</span>
                    <x-icon name="heroicon-o-check" class="size-4 text-error" x-show="getStatus(activeCell.studentId, activeCell.iso) === 2" />
                </button>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 3)" class="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium transition hover:bg-warning/10 hover:text-warning">
                    <span class="flex size-6 items-center justify-center rounded-md bg-warning text-warning-content"><x-icon name="heroicon-o-clock" class="size-3.5" /></span>
                    <span class="flex-1">Gecikdi</span>
                    <x-icon name="heroicon-o-check" class="size-4 text-warning" x-show="getStatus(activeCell.studentId, activeCell.iso) === 3" />
                </button>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 4)" class="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium transition hover:bg-info/10 hover:text-info">
                    <span class="flex size-6 items-center justify-center rounded-md bg-info text-info-content"><x-icon name="heroicon-o-exclamation-triangle" class="size-3.5" /></span>
                    <span class="flex-1">Üzrlü</span>
                    <x-icon name="heroicon-o-check" class="size-4 text-info" x-show="getStatus(activeCell.studentId, activeCell.iso) === 4" />
                </button>
                <div class="my-1 border-t border-base-200"></div>
                <button type="button" @click="selectStatus(activeCell.studentId, activeCell.iso, 0)" class="flex w-full items-center gap-2.5 px-3 py-2 text-left text-sm font-medium text-base-content/60 transition hover:bg-base-200">
                    <span class="flex size-6 items-center justify-center rounded-md bg-base-200 text-base-content/50"><x-icon name="heroicon-o-minus" class="size-3.5" /></span>
                    <span class="flex-1">Qeyd yoxdur</span>
                    <x-icon name="heroicon-o-check" class="size-4 text-base-content/50" x-show="getStatus(activeCell.studentId, activeCell.iso) === 0" />
                </button>
            </div>
        </div>
    </template>
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/attendance/controller.js')
@endpush
