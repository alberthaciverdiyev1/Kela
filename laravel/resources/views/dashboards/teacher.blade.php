@extends('layouts.teacher')
@section('title', 'Teacher Panel - Kela')
@section('content')
<div class="space-y-6">
    <x-teacher.heading subtitle="İdarə paneli">Panel</x-teacher.heading>

    <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <x-teacher.card>
            <p class="text-sm text-base-content/60">Müəllimlər</p>
            <p class="mt-1 text-2xl font-bold text-base-content">{{ $teachers }}</p>
        </x-teacher.card>
        <x-teacher.card>
            <p class="text-sm text-base-content/60">Şagirdlər</p>
            <p class="mt-1 text-2xl font-bold text-base-content">{{ $students }}</p>
        </x-teacher.card>
    </div>
</div>
@endsection
