<!DOCTYPE html>
<html lang="{{ str_replace('_', '-', app()->getLocale()) }}" data-theme="filament">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="csrf-token" content="{{ csrf_token() }}">
    <title>@yield('title', config('app.name', 'Kela'))</title>
    <script>
        (function () {
            try {
                var t = localStorage.getItem('kela-theme');
                if (t !== 'filament' && t !== 'filament-dark') {
                    t = 'filament';
                }
                document.documentElement.setAttribute('data-theme', t);
            } catch (e) {}
        })();
    </script>
    @vite(['resources/css/app.css', 'resources/js/app.js'])
</head>
<body class="font-sans antialiased text-base-content min-h-screen bg-base-100">
    <div class="flex min-h-screen">

        {{-- Sol panel: fotoğraf --}}
        <div class="relative hidden overflow-hidden lg:flex lg:w-1/2 xl:w-[56%] 2xl:w-[60%]">
            <img src="/images/auth-hero.jpg" alt="Kela Platform" class="absolute inset-0 h-full w-full object-cover">
            <div class="absolute inset-0 bg-gradient-to-t from-primary/95 via-primary/40 to-primary/5"></div>

            <div class="relative flex w-full flex-col justify-between p-12 xl:p-16">
                {{-- Logo / marka --}}
                <a href="{{ route('auth.login') }}" class="flex items-center gap-3">
                    <span class="flex h-11 w-11 items-center justify-center rounded-xl bg-white/15 text-xl font-bold text-white shadow-lg backdrop-blur">K</span>
                    <span class="text-2xl font-semibold tracking-tight text-white">Kela</span>
                </a>

                {{-- Tanıtım metni --}}
                <div>
                    <h2 class="max-w-md text-4xl font-bold leading-tight tracking-tight text-white xl:text-5xl">
                        Kela Platformasına Xoş Gəlmisiniz
                    </h2>
                    <p class="mt-4 max-w-md text-lg leading-relaxed text-white/85">
                        Təhsilin gələcəyini bizimlə formalaşdırın. Dərslərinizi, quizlərinizi və iş sahələrinizi rahatlıqla idarə edin.
                    </p>

                    <div class="mt-8 grid max-w-md grid-cols-3 gap-3">
                        <div class="rounded-2xl bg-white/10 px-4 py-3 backdrop-blur">
                            <x-icon name="heroicon-o-academic-cap" class="h-6 w-6 text-white" />
                            <p class="mt-2 text-sm font-medium text-white">Dərslər</p>
                        </div>
                        <div class="rounded-2xl bg-white/10 px-4 py-3 backdrop-blur">
                            <x-icon name="heroicon-o-clipboard-document-list" class="h-6 w-6 text-white" />
                            <p class="mt-2 text-sm font-medium text-white">Quizlər</p>
                        </div>
                        <div class="rounded-2xl bg-white/10 px-4 py-3 backdrop-blur">
                            <x-icon name="heroicon-o-briefcase" class="h-6 w-6 text-white" />
                            <p class="mt-2 text-sm font-medium text-white">İş sahələri</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        {{-- Sağ panel: form --}}
        <div class="flex flex-1 flex-col justify-center px-6 py-12 sm:px-12 lg:px-16">
            <div class="mx-auto w-full max-w-md">
                {{-- Mobil görünümde marka --}}
                <a href="{{ route('auth.login') }}" class="mb-10 flex items-center gap-3 lg:hidden">
                    <span class="flex h-11 w-11 items-center justify-center rounded-xl bg-primary text-xl font-bold text-primary-content shadow-lg">K</span>
                    <span class="text-2xl font-semibold tracking-tight text-base-content">Kela</span>
                </a>

                @if(session('status'))
                    <div class="alert alert-success mb-6">{{ session('status') }}</div>
                @endif
                @yield('content')
            </div>
        </div>

    </div>

    @stack('scripts')
</body>
</html>
