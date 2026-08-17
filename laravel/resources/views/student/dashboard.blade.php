@extends('common.layouts.app')
@section('title', 'Student Panel - Kela')
@section('content')
@php
    $user = auth()->user();
    $hour = now()->hour;
    $greeting = $hour < 12 ? 'Sabahınız xeyir' : ($hour < 18 ? 'Gününüz xeyir' : 'Axşamınız xeyir');
@endphp
<div class="space-y-6">
    {{-- Qarşılama hero --}}
    <div class="relative overflow-hidden rounded-3xl shadow-lg shadow-primary/10">
        <img src="/images/auth-hero.jpg" alt="" class="absolute inset-0 h-full w-full object-cover">
        <div class="absolute inset-0 bg-gradient-to-r from-primary/95 via-primary/70 to-secondary/40"></div>
        <div class="relative flex flex-col justify-between gap-6 p-8 sm:p-10 lg:flex-row lg:items-center">
            <div>
                <h1 class="text-3xl font-bold tracking-tight text-white sm:text-4xl">{{ $greeting }}, {{ $user->first_name }}! 👋</h1>
                <p class="mt-3 max-w-xl text-base leading-relaxed text-white/85">
                    Dərslərinizə davam edin və qeydlərinizi izləyin.
                </p>
            </div>
            <div class="flex flex-wrap gap-2 lg:justify-end">
                <a href="{{ route('student.notes.index') }}" class="inline-flex items-center gap-2 rounded-xl bg-white/15 px-4 py-2.5 text-sm font-semibold text-white shadow-sm backdrop-blur transition hover:bg-white/25">
                    <x-icon name="heroicon-o-pencil-square" class="size-4" /> Qeydlərim
                </a>
            </div>
        </div>
    </div>

    <div class="grid gap-4 sm:grid-cols-2">
        <div class="rounded-2xl border border-base-300/80 bg-base-100 p-6 shadow-sm shadow-base-300/20">
            <span class="flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-primary to-secondary text-white shadow-md shadow-primary/20">
                <x-icon name="heroicon-o-academic-cap" class="size-6" />
            </span>
            <h2 class="mt-4 font-semibold text-base-content">Dərslər</h2>
            <p class="mt-1 text-sm text-base-content/60">Dərslərinizi buradan davam etdirin.</p>
            <a href="#" class="mt-3 inline-flex items-center gap-1 text-sm font-medium text-primary hover:underline">
                Keç <x-icon name="heroicon-o-arrow-right" class="size-3.5" />
            </a>
        </div>
        <div class="rounded-2xl border border-base-300/80 bg-base-100 p-6 shadow-sm shadow-base-300/20">
            <span class="flex size-12 items-center justify-center rounded-2xl bg-gradient-to-br from-violet-500 to-purple-500 text-white shadow-md shadow-violet-500/20">
                <x-icon name="heroicon-o-pencil-square" class="size-6" />
            </span>
            <h2 class="mt-4 font-semibold text-base-content">Qeydlər</h2>
            <p class="mt-1 text-sm text-base-content/60">Qeydlərinizi yazın, rəngləyin və sabitləyin.</p>
            <a href="{{ route('student.notes.index') }}" class="mt-3 inline-flex items-center gap-1 text-sm font-medium text-primary hover:underline">
                Keç <x-icon name="heroicon-o-arrow-right" class="size-3.5" />
            </a>
        </div>
    </div>
</div>
@endsection
