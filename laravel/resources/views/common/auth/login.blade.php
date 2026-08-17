@extends('common.layouts.guest')

@section('title', 'Daxil ol - Kela')

@section('content')
    <div class="mb-10">
        <h1 class="text-4xl font-bold tracking-tight text-base-content">Daxil ol</h1>
        <p class="mt-3 text-base text-base-content/60">Hesabınıza daxil olun və idarəetməyə başlayın.</p>
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

    <form method="POST" action="{{ route('auth.login.post') }}" class="space-y-6">
        @csrf
        <div>
            <label class="mb-2 block text-sm font-medium text-base-content/70" for="email">E-poçt ünvanı</label>
            <input id="email" type="email" name="email" 
                   class="input input-bordered input-lg w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary text-base"
                   value="{{ old('email') }}" required autofocus autocomplete="email" placeholder="ad@numune.com">
        </div>
        
        <div>
            <div class="mb-2 flex items-center justify-between">
                <label class="block text-sm font-medium text-base-content/70" for="password">Şifrə</label>
                <a href="#" class="text-sm font-medium text-primary hover:underline">Şifrəni unutmusunuz?</a>
            </div>
            <input id="password" type="password" name="password" 
                   class="input input-bordered input-lg w-full focus:border-primary focus:outline-none focus:ring-1 focus:ring-primary text-base"
                   required autocomplete="current-password" placeholder="••••••••">
        </div>
        
        <button type="submit" class="btn btn-primary btn-lg mt-4 w-full">
            Daxil ol
        </button>
    </form>

    <p class="mt-8 text-center text-sm text-base-content/60">
        Hesabınız yoxdur?
        <a href="{{ route('auth.register') }}" class="font-medium text-primary hover:underline">
            Müəllim kimi qeydiyyatdan keçin
        </a>
    </p>
@endsection
