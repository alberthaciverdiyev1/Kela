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
    <div class="h-0.5 w-full bg-primary"></div>
    <header class="navbar sticky top-0 z-40 border-b border-base-300/80 bg-base-100/85 shadow-sm backdrop-blur-lg">
        <div class="flex items-center gap-4 px-4 sm:px-6">
            <a href="/" class="flex items-center gap-2.5">
                <span class="flex size-9 items-center justify-center rounded-lg bg-primary text-base font-bold text-white">K</span>
                <span class="text-lg font-bold tracking-tight text-base-content">{{ config('app.name') }}</span>
            </a>
            @auth
                <nav class="hidden items-center gap-1 text-sm font-medium md:flex">
                    @if(auth()->user()->isAdmin())
                        <a href="{{ route('teacher.dashboard') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Panel</a>
                        <div class="relative" x-data="{ open: false }" @click.outside="open = false" @keydown.escape.window="open = false">
                            <button @click="open = !open" class="flex items-center gap-1.5 rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content" :class="open ? 'bg-base-300/50 text-base-content' : ''">
                                İdarəetmə
                                <x-icon name="heroicon-o-chevron-down" class="size-3.5 opacity-60 transition-transform" x-bind:class="open ? 'rotate-180' : ''" />
                            </button>
                            <div x-cloak x-show="open" x-transition.opacity.duration.150ms class="absolute left-0 top-full mt-1.5 w-48 rounded-xl border border-base-200 bg-base-100 p-1.5 shadow-lg">
                                <a href="#" class="block rounded-lg px-3 py-2 hover:bg-base-200/60">Dərslər</a>
                                <a href="#" class="block rounded-lg px-3 py-2 hover:bg-base-200/60">Sual Bankası</a>
                            </div>
                        </div>
                    @elseif(auth()->user()->isTeacher())
                        <a href="{{ route('teacher.dashboard') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Panel</a>
                        <a href="{{ route('teacher.students.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Şagirdlər</a>
                        <a href="{{ route('teacher.workspaces.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Siniflər</a>
                        <a href="{{ route('teacher.notes.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Qeydlər</a>
                        <a href="{{ route('teacher.payments.index') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Ödənişlər</a>
                        <div class="relative" x-data="{ open: false }" @click.outside="open = false" @keydown.escape.window="open = false">
                            <button @click="open = !open" class="flex items-center gap-1.5 rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content" :class="open ? 'bg-base-300/50 text-base-content' : ''">
                                Tədris
                                <x-icon name="heroicon-o-chevron-down" class="size-3.5 opacity-60 transition-transform" x-bind:class="open ? 'rotate-180' : ''" />
                            </button>
                            <div x-cloak x-show="open" x-transition.opacity.duration.150ms class="absolute left-0 top-full mt-1.5 w-48 rounded-xl border border-base-200 bg-base-100 p-1.5 shadow-lg">
                                <a href="{{ route('teacher.lessons.index') }}" class="block rounded-lg px-3 py-2 hover:bg-base-200/60">Dərslər</a>
                                <a href="{{ route('teacher.quizzes.index') }}" class="block rounded-lg px-3 py-2 hover:bg-base-200/60">Quizlər</a>
                                <a href="{{ route('teacher.questions.index') }}" class="block rounded-lg px-3 py-2 hover:bg-base-200/60">Sual Bankı</a>
                                <a href="{{ route('teacher.attendance.index') }}" class="block rounded-lg px-3 py-2 hover:bg-base-200/60">Davam</a>
                            </div>
                        </div>
                    @elseif(auth()->user()->isStudent())
                        <a href="{{ route('student.dashboard') }}" class="rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content">Panel</a>
                        <div class="relative" x-data="{ open: false }" @click.outside="open = false" @keydown.escape.window="open = false">
                            <button @click="open = !open" class="flex items-center gap-1.5 rounded-xl px-3 py-2 text-base-content/65 transition hover:bg-base-300/50 hover:text-base-content" :class="open ? 'bg-base-300/50 text-base-content' : ''">
                                Tədris Materialları
                                <x-icon name="heroicon-o-chevron-down" class="size-3.5 opacity-60 transition-transform" x-bind:class="open ? 'rotate-180' : ''" />
                            </button>
                            <div x-cloak x-show="open" x-transition.opacity.duration.150ms class="absolute left-0 top-full mt-1.5 w-48 rounded-xl border border-base-200 bg-base-100 p-1.5 shadow-lg">
                                <a href="{{ route('student.notes.index') }}" class="block rounded-lg px-3 py-2 hover:bg-base-200/60">Qeydlər</a>
                            </div>
                        </div>
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
                            @if(auth()->user()->avatar_url)
                                <img src="{{ auth()->user()->avatar_url }}" alt="{{ auth()->user()->full_name }}" class="size-8 rounded-full object-cover ring-2 ring-primary/20" />
                            @else
                                <span class="flex size-8 items-center justify-center rounded-full bg-primary text-xs font-bold text-white">
                                    {{ initials(auth()->user()->first_name, auth()->user()->last_name) }}
                                </span>
                            @endif
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
                            <div class="flex items-center gap-3 border-b border-base-200 bg-base-200/40 px-4 py-3.5">
                                @if(auth()->user()->avatar_url)
                                    <img src="{{ auth()->user()->avatar_url }}" alt="{{ auth()->user()->full_name }}" class="size-10 shrink-0 rounded-full object-cover ring-2 ring-primary/20" />
                                @else
                                    <span class="flex size-10 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-bold text-white">
                                        {{ initials(auth()->user()->first_name, auth()->user()->last_name) }}
                                    </span>
                                @endif
                                <div class="min-w-0">
                                    <p class="truncate text-sm font-semibold text-base-content">{{ auth()->user()->full_name }}</p>
                                    <p class="truncate text-xs text-base-content/50">{{ auth()->user()->email }}</p>
                                </div>
                            </div>

                            <div class="border-t border-base-200 p-1.5">
                                <div class="py-1">
                                    <a href="{{ route('profile.edit') }}" class="flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium text-base-content transition hover:bg-base-200">
                                        <x-icon name="heroicon-o-user" class="size-4 opacity-70" />
                                        Profil
                                    </a>
                                </div>
                                <form method="POST" action="{{ route('auth.logout') }}" class="border-t border-base-200 mt-1.5 pt-1.5">
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
