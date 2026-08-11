@extends('common.layouts.teacher')
@section('title', 'Şagirdlər - Kela')
@section('content')
@php
    $studentConfig = [
        'fragmentUrl' => $fragmentUrl,
        'cities' => $cities,
        'statuses' => $statuses,
    ];
@endphp
<div
    class="space-y-6"
    x-data="studentManager({{ \Illuminate\Support\Js::from($studentConfig) }})"
    @keydown.escape.window="showForm = false"
>
    <x-teacher.heading subtitle="Şagirdləri idarə et">
        Şagirdlər
        <x-slot:actions>
            <button type="button" class="btn btn-primary" @click="openAdd()">
                <x-icon name="heroicon-o-plus" class="size-4" /> Yeni Şagird
            </button>
        </x-slot:actions>
    </x-teacher.heading>

    <x-teacher.card :padding="false">
        <form method="GET" action="{{ route('teacher.students.index') }}" class="flex items-center gap-3 border-b border-base-300 p-4">
            <div class="relative w-72">
                <span class="pointer-events-none absolute inset-y-0 left-3 flex items-center">
                    <x-icon name="heroicon-o-magnifying-glass" class="size-4 text-base-content/40" />
                </span>
                <x-teacher.input name="search" value="{{ $search }}" placeholder="Ad, soyad, e-poçt..." class="pl-9" />
            </div>
            @if ($search !== '')
                <a href="{{ route('teacher.students.index') }}" class="btn btn-ghost btn-sm">Təmizlə</a>
            @endif
        </form>

        <div id="students-table" x-ref="table" @click="onTableClick($event)">
            @include('teacher.students._table', ['students' => $students])
        </div>
    </x-teacher.card>

    {{-- Add / Edit dialog --}}
    <div x-show="showForm" x-transition.opacity class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
        <div class="max-h-[90vh] w-full max-w-2xl overflow-y-auto rounded-xl border border-base-300 bg-base-100 p-6 shadow-xl">
            <h3 x-text="formTitle" class="mb-4 text-lg font-semibold text-base-content">Yeni Şagird</h3>
            <div class="grid gap-4 sm:grid-cols-2">
                {{-- Qeyd: x-model yoxdur — form dəyərləri reaktiv saxlanılmır.
                     Dəyərlər add/edit modulları tərəfindən $refs (DOM) vasitəsilə
                     oxunub yazılır ki, index.js-də form dəyişəni olmasın. --}}
                <x-teacher.field label="Ad" name="first_name" :required="true">
                    <x-teacher.input name="first_name" x-ref="firstName" />
                </x-teacher.field>

                <x-teacher.field label="Soyad" name="last_name">
                    <x-teacher.input name="last_name" x-ref="lastName" />
                </x-teacher.field>

                <x-teacher.field label="E-poçt" name="email" :required="true" class="sm:col-span-2">
                    <x-teacher.input name="email" type="email" x-ref="email" />
                </x-teacher.field>

                <x-teacher.field
                    label="Şifrə"
                    name="password"
                    :hint="'Yeni şagirddə tələb olunur; redaktədə boş qalsa dəyişməz.'"
                    class="sm:col-span-2"
                >
                    <x-teacher.input name="password" type="password" x-ref="password" autocomplete="new-password" />
                </x-teacher.field>

                <x-teacher.field label="Şəhər" name="city_id">
                    <select name="city_id" x-ref="cityId" class="select select-bordered w-full text-sm">
                        <option value="">Seçin...</option>
                        <template x-for="(label, value) in cities" :key="value">
                            <option :value="value" x-text="label"></option>
                        </template>
                    </select>
                </x-teacher.field>

                <x-teacher.field label="Doğum tarixi" name="birth_date">
                    <x-teacher.input name="birth_date" type="date" x-ref="birthDate" />
                </x-teacher.field>

                <x-teacher.field label="Status" name="status">
                    <select name="status" x-ref="status" class="select select-bordered w-full text-sm">
                        <template x-for="(label, value) in statuses" :key="value">
                            <option :value="value" x-text="label"></option>
                        </template>
                    </select>
                </x-teacher.field>
            </div>
            <div class="mt-5 flex items-center justify-end gap-2 border-t border-base-300 pt-4">
                <button type="button" class="btn btn-sm btn-ghost" @click="showForm = false">Ləğv et</button>
                <button type="button" class="btn btn-sm btn-primary" @click="save()">Yadda Saxla</button>
            </div>
        </div>
    </div>
</div>
@endsection

@push('scripts')
    @vite('resources/js/teacher/student/controller.js')
@endpush
