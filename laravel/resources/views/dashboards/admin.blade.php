@extends('layouts.app')
@section('title', 'Admin Panel - Kela')
@section('content')
    <h1 class="text-2xl font-bold">Xoş gəldin, {{ auth()->user()->full_name }}!</h1>
    <p class="text-base-content/60 mb-6">Admin paneli</p>
    <div class="grid gap-4 sm:grid-cols-2">
        <div class="card bg-base-100 border border-base-300">
            <div class="card-body">
                <span class="text-sm text-base-content/50">Müəllimlər</span>
                <span class="text-2xl font-bold">{{ $teachers }}</span>
            </div>
        </div>
        <div class="card bg-base-100 border border-base-300">
            <div class="card-body">
                <span class="text-sm text-base-content/50">Şagirdlər</span>
                <span class="text-2xl font-bold">{{ $students }}</span>
            </div>
        </div>
    </div>
@endsection
