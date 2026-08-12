@extends('common.layouts.app')

@section('title', 'Müəllim Qeydiyyatı - Kela')

@section('content')
<div class="mx-auto mt-16 max-w-sm">
    <div class="card border border-base-300 bg-base-100 shadow-md">
        <div class="card-body p-6">
            <h1 class="text-xl font-bold mb-1">Kela</h1>
            <p class="text-sm text-base-content/60 mb-6">Müəllim hesabı yaradın</p>

            @if($errors->any())
                <div class="alert alert-error mb-4 text-sm">
                    {{ $errors->first() }}
                </div>
            @endif

            <form method="POST" action="{{ route('auth.register.post') }}">
                @csrf
                <div class="field mb-4">
                    <label class="label" for="first_name">Ad</label>
                    <input id="first_name" type="text" name="first_name" class="input w-full"
                           value="{{ old('first_name') }}" required autofocus autocomplete="given-name">
                </div>
                <div class="field mb-4">
                    <label class="label" for="last_name">Soyad</label>
                    <input id="last_name" type="text" name="last_name" class="input w-full"
                           value="{{ old('last_name') }}" autocomplete="family-name">
                </div>
                <div class="field mb-4">
                    <label class="label" for="email">E-poçt</label>
                    <input id="email" type="email" name="email" class="input w-full"
                           value="{{ old('email') }}" required autocomplete="email">
                </div>
                <div class="field mb-4">
                    <label class="label" for="password">Şifrə</label>
                    <input id="password" type="password" name="password" class="input w-full"
                           required autocomplete="new-password">
                </div>
                <div class="field mb-6">
                    <label class="label" for="password_confirmation">Şifrə (təkrar)</label>
                    <input id="password_confirmation" type="password" name="password_confirmation" class="input w-full"
                           required autocomplete="new-password">
                </div>
                <button type="submit" class="btn btn-primary w-full">Qeydiyyatdan keç</button>
            </form>

            <p class="mt-4 text-center text-sm text-base-content/60">
                Artıq hesabınız var?
                <a href="{{ route('auth.login') }}" class="font-medium text-primary hover:underline">Daxil olun</a>
            </p>
        </div>
    </div>
</div>
@endsection
