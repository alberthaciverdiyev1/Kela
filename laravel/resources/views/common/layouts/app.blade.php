<!DOCTYPE html>
<html lang="{{ str_replace('_', '-', app()->getLocale()) }}" data-theme="filament">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="csrf-token" content="{{ csrf_token() }}">
    <title>@yield('title', config('app.name', 'Kela'))</title>
    <script>
        // FOUC-un qarşısını alır: saxlanılmış tema varsa tətbiq olunur.
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
<body class="font-sans antialiased bg-base-200">
    <div class="h-1 w-full bg-gradient-to-r from-primary via-secondary to-accent"></div>
    <header class="navbar sticky top-0 z-40 border-b border-base-300/80 bg-base-100/85 shadow-sm backdrop-blur-lg">
        <div class="flex items-center gap-4 px-4 sm:px-6">
            <a href="/" class="flex items-center gap-2.5">
                <span class="flex size-9 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-secondary text-base font-bold text-white shadow-md shadow-primary/25">K</span>
                <span class="text-lg font-bold tracking-tight text-base-content">{{ config('app.name') }}</span>
            </a>
            @auth
                <nav class="hidden items-center gap-1 text-sm font-medium md:flex">
                    @if(auth()->user()->isAdmin())
                        <a href="{{ route('teacher.dashboard') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Panel</a>
                        <a href="#" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Dərslər</a>
                        <a href="#" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Sual Bankası</a>
                    @elseif(auth()->user()->isTeacher())
                        <a href="{{ route('teacher.dashboard') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Panel</a>
                        <a href="{{ route('teacher.lessons.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Dərslər</a>
                        <a href="{{ route('teacher.quizzes.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Quizlər</a>
                        <a href="{{ route('teacher.workspaces.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">İş Sahələri</a>
                    @elseif(auth()->user()->isStudent())
                        <a href="{{ route('student.dashboard') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Dərslər</a>
                        <a href="{{ route('student.notes.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Qeydlər</a>
                    @endif
                </nav>
            @endauth
            <div class="ms-auto flex items-center gap-2.5">
                @auth
                    <span class="hidden items-center gap-2 sm:flex">
                        <span class="flex size-8 items-center justify-center rounded-full bg-gradient-to-br from-primary to-secondary text-xs font-bold text-white shadow-sm">
                            {{ strtoupper(mb_substr(auth()->user()->first_name, 0, 1).mb_substr(auth()->user()->last_name, 0, 1)) }}
                        </span>
                        <span class="text-sm font-medium text-base-content/80">{{ auth()->user()->full_name }}</span>
                    </span>
                    <form method="POST" action="{{ route('auth.logout') }}">
                        @csrf
                        <button class="btn btn-ghost btn-sm gap-1.5 text-base-content/70 hover:bg-error/10 hover:text-error" type="submit">
                            <x-icon name="heroicon-o-arrow-right-start-on-rectangle" class="size-4" />
                            <span class="hidden sm:inline">Çıxış</span>
                        </button>
                    </form>
                @endauth
            </div>
        </div>
    </header>
    <main class="relative mx-auto max-w-6xl p-6">
        <div aria-hidden="true" class="pointer-events-none absolute inset-x-0 top-0 h-64 bg-gradient-to-b from-primary/[0.07] via-secondary/[0.04] to-transparent"></div>
        <div class="relative">
            @if(session('status'))
                <div class="alert alert-success mb-4">{{ session('status') }}</div>
            @endif
            @yield('content')
        </div>
    </main>

    @stack('scripts')
</body>
</html>
