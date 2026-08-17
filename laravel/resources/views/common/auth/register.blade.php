@extends('common.layouts.guest')

@section('title', 'Müəllim Qeydiyyatı - Kela')

@section('content')
    <div class="mb-10">
        <h1 class="text-4xl font-bold tracking-tight text-base-content">Qeydiyyat</h1>
        <p class="mt-3 text-base text-base-content/60">Kela platformasına qoşulun və dərslərinizi idarə edin.</p>
    </div>

    @if($errors->any())
        <div class="mb-6 rounded-xl bg-error/10 p-4 text-sm text-error">
            <div class="flex items-center gap-2 font-medium">
                <x-icon name="heroicon-o-exclamation-triangle" class="h-5 w-5 shrink-0" />
                <span>Xəta baş verdi</span>
            </div>
            <p class="ml-7 mt-1 text-error/80">{{ $errors->first() }}</p>
        </div>
    @endif

    <form method="POST" action="{{ route('auth.register.post') }}" class="space-y-6">
        @csrf
        
        <div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
            <div>
                <label class="mb-2 block text-sm font-medium text-base-content/70" for="first_name">Ad</label>
                <input id="first_name" type="text" name="first_name" 
                       class="input input-bordered input-lg w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary text-base"
                       value="{{ old('first_name') }}" required autofocus autocomplete="given-name">
            </div>
            <div>
                <label class="mb-2 block text-sm font-medium text-base-content/70" for="last_name">Soyad</label>
                <input id="last_name" type="text" name="last_name" 
                       class="input input-bordered input-lg w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary text-base"
                       value="{{ old('last_name') }}" autocomplete="family-name">
            </div>
        </div>

        <div>
            <label class="mb-2 block text-sm font-medium text-base-content/70" for="email">E-poçt ünvanı</label>
            <input id="email" type="email" name="email" 
                   class="input input-bordered input-lg w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary text-base"
                   value="{{ old('email') }}" required autocomplete="email" placeholder="ad@numune.com">
        </div>
        
        <div class="grid grid-cols-1 gap-6 sm:grid-cols-2">
            <div>
                <label class="mb-2 block text-sm font-medium text-base-content/70" for="password">Şifrə</label>
                <input id="password" type="password" name="password" 
                       class="input input-bordered input-lg w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary text-base"
                       required autocomplete="new-password">
            </div>
            <div>
                <label class="mb-2 block text-sm font-medium text-base-content/70" for="password_confirmation">Şifrə (təkrar)</label>
                <input id="password_confirmation" type="password" name="password_confirmation" 
                       class="input input-bordered input-lg w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary text-base"
                       required autocomplete="new-password">
            </div>
        </div>
        
        <button type="submit" class="btn btn-primary btn-lg mt-4 w-full">
            Qeydiyyatdan keç
        </button>
    </form>

    <p class="mt-8 text-center text-sm text-base-content/60">
        Artıq hesabınız var?
        <a href="{{ route('auth.login') }}" class="font-medium text-primary hover:underline">
            Daxil olun
        </a>
    </p>
@endsection
