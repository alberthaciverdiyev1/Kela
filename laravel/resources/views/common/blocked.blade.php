@extends('common.layouts.app')
@section('title', 'No Access - Kela')
@section('content')
    <div class="card mx-auto mt-16 max-w-md border border-base-300 bg-base-100 text-center">
        <div class="card-body">
            <h1 class="text-xl font-bold">No Access</h1>
            <p class="text-sm text-base-content/60">Siz bu panele giriş üçün icazəniz yoxdur.</p>
            <div class="mt-4">
                <a href="{{ auth()->user()->homeRoute() }}" class="btn btn-primary">Panele dön</a>
            </div>
        </div>
    </div>
@endsection
