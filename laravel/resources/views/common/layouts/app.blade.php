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
                    {{-- Profil dropdown --}}
                    <div class="relative" x-data="{ open: false }" @click.outside="open = false" @keydown.escape.window="open = false">
                        <button
                            type="button"
                            @click="open = !open"
                            class="flex items-center gap-2 rounded-xl px-2 py-1.5 transition hover:bg-base-300/50"
                            aria-haspopup="menu"
                            x-bind:aria-expanded="open ? 'true' : 'false'"
                        >
                            <span class="flex size-8 items-center justify-center rounded-full bg-gradient-to-br from-primary to-secondary text-xs font-bold text-white shadow-sm">
                                {{ strtoupper(mb_substr(auth()->user()->first_name, 0, 1).mb_substr(auth()->user()->last_name, 0, 1)) }}
                            </span>
                            <span class="hidden text-sm font-medium text-base-content/80 sm:block">{{ auth()->user()->full_name }}</span>
                            <x-icon name="heroicon-o-chevron-down" class="size-4 text-base-content/40 transition-transform duration-200" x-bind:class="open ? 'rotate-180' : ''" />
                        </button>

                        <div
                            x-cloak
                            x-show="open"
                            x-transition:enter="transition ease-out duration-150"
                            x-transition:enter-start="opacity-0 translate-y-1 scale-95"
                            x-transition:enter-end="opacity-100 translate-y-0 scale-100"
                            x-transition:leave="transition ease-in duration-100"
                            x-transition:leave-start="opacity-100 translate-y-0 scale-100"
                            x-transition:leave-end="opacity-0 translate-y-1 scale-95"
                            class="absolute right-0 z-50 mt-2 w-60 overflow-hidden rounded-2xl border border-base-300/80 bg-base-100 shadow-xl shadow-base-300/30"
                            role="menu"
                        >
                            <div class="flex items-center gap-3 border-b border-base-200 bg-gradient-to-r from-primary/[0.06] to-secondary/[0.04] px-4 py-3.5">
                                <span class="flex size-10 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-primary to-secondary text-sm font-bold text-white shadow-md shadow-primary/20">
                                    {{ strtoupper(mb_substr(auth()->user()->first_name, 0, 1).mb_substr(auth()->user()->last_name, 0, 1)) }}
                                </span>
                                <div class="min-w-0">
                                    <p class="truncate text-sm font-semibold text-base-content">{{ auth()->user()->full_name }}</p>
                                    <p class="truncate text-xs text-base-content/50">{{ auth()->user()->email }}</p>
                                </div>
                            </div>

                            <div class="border-t border-base-200 p-1.5">
                                <form method="POST" action="{{ route('auth.logout') }}">
                                    @csrf
                                    <button type="submit" class="flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-error transition hover:bg-error/10">
                                        <x-icon name="heroicon-o-arrow-right-start-on-rectangle" class="size-4" />
                                        Çıxış
                                    </button>
                                </form>
                            </div>
                        </div>
                    </div>
                @endauth
            </div>
        </div>
    </header>
    <main class="relative flex-1" @style(['width: 100%' => config('theme.full_bleed', true), 'max-width: 72rem; margin-inline: auto; width: 100%' => ! config('theme.full_bleed', true)])>
        <div @style([
            'padding-left: '.config('theme.side_padding', 40).'px',
            'padding-right: '.config('theme.side_padding', 40).'px',
        ]) class="relative">
            <div aria-hidden="true" class="pointer-events-none absolute inset-x-0 top-0 h-64 bg-gradient-to-b from-primary/[0.07] via-secondary/[0.04] to-transparent"></div>
            <div class="relative py-6">
                @if(session('status'))
                    <div class="alert alert-success mb-4">{{ session('status') }}</div>
                @endif
                @yield('content')
            </div>
        </div>
    </main>

    @stack('scripts')
</body>
</html>
