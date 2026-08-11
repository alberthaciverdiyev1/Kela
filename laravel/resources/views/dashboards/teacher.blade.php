@extends('layouts.app')
@section('title', 'Teacher Panel - Kela')
@section('content')
    <h1 class="text-2xl font-bold">Xoş gəldin, {{ auth()->user()->full_name }}!</h1>
    <p class="text-base-content/60 mb-6">Müəllim paneli</p>
    <div class="grid gap-4 sm:grid-cols-3">
        <div class="card bg-base-100 border border-base-300">
            <div class="card-body">
                <span class="text-sm text-base-content/50">İş Sahələri</span>
                <span class="text-2xl font-bold">{{ $workspaceCount }}</span>
            </div>
        </div>
        <div class="card bg-base-100 border border-base-300">
            <div class="card-body">
                <span class="text-sm text-base-content/50">Şagirdlər</span>
                <span class="text-2xl font-bold">{{ $studentCount }}</span>
            </div>
        </div>
        <div class="card bg-base-100 border border-base-300">
            <div class="card-body">
                <span class="text-sm text-base-content/50">Məzmun</span>
                <span class="text-2xl font-bold">{{ $contentCount }}</span>
            </div>
        </div>
    </div>
@endsection
