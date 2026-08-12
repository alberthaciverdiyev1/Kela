@extends('common.layouts.teacher')
@section('title', $workspaceName.' - Kela')
@section('content')
@php
    $workspaceConfig = [
        'workspaceId' => (int) $workspaceId,
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

    {{-- Students --}}
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

    {{-- Student dialog --}}
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
