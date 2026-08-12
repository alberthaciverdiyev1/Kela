@extends('common.layouts.app')

@section('title', 'Login - Kela')

@section('content')
<div class="mx-auto mt-16 max-w-sm">
    <div class="card border border-base-300 bg-base-100 shadow-md">
        <div class="card-body p-6">
            <h1 class="text-xl font-bold mb-1">Kela</h1>
            <p class="text-sm text-base-content/60 mb-6">Hesabınıza daxil olun</p>

            @if($errors->any())
                <div class="alert alert-error mb-4 text-sm">
                    {{ $errors->first() }}
                </div>
            @endif

            <form method="POST" action="{{ route('auth.login.post') }}">
                @csrf
                <div class="field mb-4">
                    <label class="label" for="email">E-poçt</label>
                    <input id="email" type="email" name="email" class="input w-full"
                           value="{{ old('email') }}" required autofocus autocomplete="email">
                </div>
                <div class="field mb-6">
                    <label class="label" for="password">Şifrə</label>
                    <input id="password" type="password" name="password" class="input w-full"
                           required autocomplete="current-password">
                </div>
                <button type="submit" class="btn btn-primary w-full">Daxil ol</button>
            </form>

            <p class="mt-4 text-center text-sm text-base-content/60">
                Hesabınız yoxdur?
                <a href="{{ route('auth.register') }}" class="font-medium text-primary hover:underline">Müəllim qeydiyyatı</a>
            </p>
        </div>
    </div>
</div>
@endsection
