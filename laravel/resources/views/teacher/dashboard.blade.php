@extends('common.layouts.teacher')
@section('title', 'Teacher Panel - Kela')
@section('content')
@php
    $user = auth()->user();
    $hour = now()->hour;
    $greeting = $hour < 12 ? 'Sabahınız xeyir' : ($hour < 18 ? 'Gününüz xeyir' : 'Axşamınız xeyir');
    $dateLabel = now()->translatedFormat('j F Y, l');
@endphp

<div class="space-y-6">

    {{-- Qarşılama hero (login-in foto + gradyan dili) --}}
    <div class="relative overflow-hidden rounded-3xl shadow-lg">
        <img src="/images/auth-hero.jpg" alt="" class="absolute inset-0 h-full w-full object-cover">
        <div class="absolute inset-0 bg-gradient-to-r from-primary/95 via-primary/70 to-secondary/40"></div>
        <div class="relative flex flex-col justify-between gap-6 p-8 sm:p-10 lg:flex-row lg:items-center">
            <div>
                <p class="text-sm font-medium uppercase tracking-widest text-white/70">{{ $dateLabel }}</p>
                <h1 class="mt-2 text-3xl font-bold tracking-tight text-white sm:text-4xl">{{ $greeting }}, {{ $user->first_name }}!</h1>
                <p class="mt-3 max-w-xl text-base leading-relaxed text-white/85">
                    Kela platformasına xoş gəlmisiniz. Dərslərinizi, quizlərinizi və iş sahələrinizi buradan idarə edin.
                </p>
            </div>
            <div class="flex flex-wrap gap-2 lg:justify-end">
                <a href="{{ route('teacher.lessons.create') }}" class="inline-flex items-center gap-2 rounded-xl bg-white/15 px-4 py-2.5 text-sm font-semibold text-white shadow-sm backdrop-blur transition hover:bg-white/25">
                    <x-icon name="heroicon-o-plus" class="size-4" /> Yeni Dərs
                </a>
                <a href="{{ route('teacher.quizzes.create') }}" class="inline-flex items-center gap-2 rounded-xl bg-white/15 px-4 py-2.5 text-sm font-semibold text-white shadow-sm backdrop-blur transition hover:bg-white/25">
                    <x-icon name="heroicon-o-plus" class="size-4" /> Yeni Quiz
                </a>
            </div>
        </div>
    </div>

    {{-- Stat kartları --}}
    <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <x-teacher.stat-card label="Şagirdlər" :value="$students" icon="users" href="{{ route('teacher.students.index') }}">
            Bütün şagirdlər
        </x-teacher.stat-card>
        <x-teacher.stat-card label="Dərslər" :value="$lessons" icon="academic-cap" accent="bg-emerald-600" href="{{ route('teacher.lessons.index') }}">
            Dərsləri aç
        </x-teacher.stat-card>
        <x-teacher.stat-card label="Quizlər" :value="$quizzes" icon="clipboard-document-list" accent="bg-amber-500" href="{{ route('teacher.quizzes.index') }}">
            Quizləri aç
        </x-teacher.stat-card>
        <x-teacher.stat-card label="Siniflər" :value="$workspaces" icon="building-office-2" accent="bg-violet-600" href="{{ route('teacher.workspaces.index') }}">
            İş sahələrini aç
        </x-teacher.stat-card>
    </div>

    {{-- Sürətli əməliyyatlar + statik panel --}}
    <div class="grid gap-6 lg:grid-cols-3">
        {{-- Sürətli əməliyyatlar --}}
        <div class="lg:col-span-2">
            <h2 class="mb-3 text-lg font-bold tracking-tight text-base-content">Sürətli Əməliyyatlar</h2>
            <div class="grid gap-4 sm:grid-cols-2">
                <x-teacher.action-card title="Yeni Dərs" description="Video və ya qeyd dərsi əlavə edin" icon="video-camera" href="{{ route('teacher.lessons.create') }}" />
                <x-teacher.action-card title="Yeni Quiz" description="Sual bankından quiz tərtib edin" icon="clipboard-document-list" accent="bg-amber-500" href="{{ route('teacher.quizzes.create') }}" />
                <x-teacher.action-card title="Yeni Sinif" description="Şagirdləri bir sinifdə toplayın" icon="building-office-2" accent="bg-violet-600" href="{{ route('teacher.workspaces.index', ['create' => 1]) }}" />
                <x-teacher.action-card title="Yeni Şagird" description="Sistemə yeni şagird əlavə edin" icon="user-plus" accent="bg-emerald-600" href="{{ route('teacher.students.create') }}" />
            </div>
        </div>

        {{-- Yan panel: müəllim özeti --}}
        <div class="space-y-4">
            <h2 class="mb-3 text-lg font-bold tracking-tight text-base-content">Müəllim Paneli</h2>

            <x-teacher.card accent="bg-primary">
                <div class="flex items-center gap-4">
                    <span class="flex size-12 items-center justify-center rounded-xl bg-primary text-lg font-bold text-white">
                        {{ strtoupper(mb_substr($user->first_name, 0, 1).mb_substr($user->last_name, 0, 1)) }}
                    </span>
                    <div>
                        <p class="font-semibold text-base-content">{{ $user->full_name }}</p>
                        <p class="text-sm text-base-content/60">{{ $user->email }}</p>
                    </div>
                </div>
                <div class="mt-5 grid grid-cols-2 gap-3">
                    <div class="rounded-xl bg-base-200/50 p-3">
                        <p class="text-xs font-medium text-base-content/60">Müəllimlər</p>
                        <p class="text-xl font-bold text-base-content">{{ $teachers }}</p>
                    </div>
                    <div class="rounded-xl bg-base-200/50 p-3">
                        <p class="text-xs font-medium text-base-content/60">Ev Tapşırığı</p>
                        <p class="text-xl font-bold text-base-content">{{ $homeworks }}</p>
                    </div>
                </div>
            </x-teacher.card>

            <x-teacher.card>
                <h3 class="text-sm font-semibold text-base-content">Tez keçidlər</h3>
                <ul class="mt-3 space-y-1">
                    <li>
                        <a href="{{ route('teacher.notes.index') }}" class="flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-base-content/70 transition hover:bg-primary/10 hover:text-primary">
                            <x-icon name="heroicon-o-pencil-square" class="size-4" /> Qeydlər
                        </a>
                    </li>
                    <li>
                        <a href="{{ route('teacher.attendance.index') }}" class="flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-base-content/70 transition hover:bg-primary/10 hover:text-primary">
                            <x-icon name="heroicon-o-calendar-days" class="size-4" /> Davam
                        </a>
                    </li>
                    <li>
                        <a href="{{ route('teacher.homeworks.index') }}" class="flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-base-content/70 transition hover:bg-primary/10 hover:text-primary">
                            <x-icon name="heroicon-o-book-open" class="size-4" /> Ev Tapşırıqları
                        </a>
                    </li>
                    <li>
                        <a href="{{ route('teacher.questions.index') }}" class="flex items-center gap-2 rounded-lg px-2 py-1.5 text-sm text-base-content/70 transition hover:bg-primary/10 hover:text-primary">
                            <x-icon name="heroicon-o-queue-list" class="size-4" /> Sual Bankı
                        </a>
                    </li>
                </ul>
            </x-teacher.card>
        </div>
    </div>
</div>
@endsection
