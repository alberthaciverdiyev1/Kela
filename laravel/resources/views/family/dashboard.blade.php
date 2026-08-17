@extends('common.layouts.app')
@section('title', 'Parent Panel - Kela')
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
        <div class="relative p-8 sm:p-10">
            <h1 class="text-3xl font-bold tracking-tight text-white sm:text-4xl">{{ $greeting }}, {{ $user->first_name }}! 👋</h1>
            <p class="mt-3 max-w-xl text-base leading-relaxed text-white/85">
                Övladınızın təhsil prosesini izlədiyiniz valideyn paneli.
            </p>
        </div>
    </div>
</div>
@endsection
