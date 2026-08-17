<!DOCTYPE html>
<html lang="{{ str_replace('_', '-', app()->getLocale()) }}" data-theme="filament">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <meta name="csrf-token" content="{{ csrf_token() }}">
    <title>@yield('title', config('app.name', 'Kela')) | Teacher Panel</title>
    <script>
        // FOUC-un qarşısını alır: saxlanılmış tema varsa səhifə rənglənməmişdən əvvəl tətbiq olunur.
        (function () {
            try {
                var t = localStorage.getItem('kela-theme');
                if (t !== 'filament' && t !== 'filament-dark') {
                    t = 'filament'; // standart açıq rejim
                }
                document.documentElement.setAttribute('data-theme', t);
            } catch (e) {
                /* localStorage əlçatan deyilsə standart açıq qalır */
            }
        })();
    </script>
    @vite(['resources/css/app.css', 'resources/js/app.js'])
    @stack('styles')
</head>
<body class="min-h-screen bg-base-200 font-sans antialiased">
    @php
        $user = auth()->user();
        $path = request()->path();
        $nav = [];
        $nav[] = ['label' => 'Müəllim Paneli', 'url' => route('teacher.dashboard'), 'active' => $path === 'teacher/dashboard', 'icon' => 'home'];
        $nav[] = ['label' => 'Şagirdlər', 'url' => route('teacher.students.index'), 'active' => str_starts_with($path, 'teacher/students'), 'icon' => 'users'];
        $nav[] = ['label' => 'Sual Bankı', 'url' => route('teacher.questions.index'), 'active' => str_starts_with($path, 'teacher/questions'), 'icon' => 'queue-list'];
        $nav[] = ['label' => 'İş Sahələri', 'url' => route('teacher.workspaces.index'), 'active' => str_starts_with($path, 'teacher/workspaces'), 'icon' => 'building-office-2'];
        $nav[] = ['label' => 'Davam', 'url' => route('teacher.attendance.index'), 'active' => str_starts_with($path, 'teacher/attendance'), 'icon' => 'calendar-days'];
        $nav[] = ['label' => 'Qeydlər', 'url' => route('teacher.notes.index'), 'active' => str_starts_with($path, 'teacher/notes'), 'icon' => 'pencil-square'];
        $nav[] = ['label' => 'Dərslər', 'url' => route('teacher.lessons.index'), 'active' => str_starts_with($path, 'teacher/lessons'), 'icon' => 'academic-cap'];
        $nav[] = ['label' => 'Quizlər', 'url' => route('teacher.quizzes.index'), 'active' => str_starts_with($path, 'teacher/quizzes'), 'icon' => 'clipboard-document-list'];
        $nav[] = ['label' => 'Ev Tapşırığı', 'url' => route('teacher.homeworks.index'), 'active' => str_starts_with($path, 'teacher/homeworks'), 'icon' => 'book-open'];
        $roleLabel = match (true) {
            $user->isAdmin() => 'Admin',
            $user->isTeacher() => 'Müəllim',
            default => 'İstifadəçi',
        };
        $initials = strtoupper(mb_substr($user->first_name, 0, 1).mb_substr($user->last_name, 0, 1));
    @endphp

    <div class="flex min-h-screen flex-col">
        {{-- Üst gradyan aksent xətti --}}
        <div class="h-1 w-full bg-gradient-to-r from-primary via-secondary to-accent"></div>

        {{-- Teacher panel üst navbar --}}
        <header class="sticky top-0 z-40 border-b border-base-300/80 bg-base-100/85 shadow-sm backdrop-blur-lg">
            <div class="grid h-16 grid-cols-[auto_1fr_auto] items-center gap-4 px-4 sm:px-6 lg:px-10">
                {{-- Logo / marka --}}
                <a href="{{ route('teacher.dashboard') }}" class="flex shrink-0 items-center gap-2.5">
                    <span class="flex size-9 items-center justify-center rounded-xl bg-gradient-to-br from-primary to-secondary text-base font-bold text-white shadow-md shadow-primary/25">K</span>
                    <span class="text-xl font-bold tracking-tight text-base-content">Kela</span>
                </a>

                {{-- Navigasiya --}}
                <nav class="flex items-center justify-center gap-0.5 overflow-x-auto text-sm font-medium">
                    @foreach($nav as $item)
                        <a href="{{ $item['url'] }}"
                           @class([
                               'group inline-flex shrink-0 items-center gap-1.5 rounded-xl px-3 py-2 transition-all duration-200',
                               'bg-gradient-to-br from-primary/10 to-secondary/10 text-primary shadow-sm' => $item['active'],
                               'text-base-content/65 hover:bg-base-300/50 hover:text-base-content' => ! $item['active'],
                           ])>
                            <x-icon name="heroicon-o-{{ $item['icon'] }}" class="size-4 opacity-70" />
                            {{ $item['label'] }}
                        </a>
                    @endforeach
                </nav>

                {{-- Sağ aksiyonlar --}}
                <div class="flex shrink-0 items-center justify-end gap-2.5">
                    <button
                        type="button"
                        x-data="{ dark: document.documentElement.getAttribute('data-theme') === 'filament-dark' }"
                        @click="
                            dark = !dark;
                            document.documentElement.setAttribute('data-theme', dark ? 'filament-dark' : 'filament');
                            localStorage.setItem('kela-theme', dark ? 'filament-dark' : 'filament');
                        "
                        class="btn btn-ghost btn-circle btn-sm hover:bg-base-300/50"
                        title="Açıq / tünd rejim"
                        aria-label="Tema dəyişdir"
                    >
                        <x-icon name="heroicon-o-moon" class="size-4" x-show="!dark" />
                        <x-icon name="heroicon-o-sun" class="size-4" x-show="dark" />
                    </button>

                    {{-- Profil dropdown --}}
                    <div class="relative" x-data="{ open: false }" @click.outside="open = false" @keydown.escape.window="open = false">
                        <button
                            type="button"
                            @click="open = !open"
                            class="flex items-center gap-2 rounded-xl px-2 py-1.5 transition hover:bg-base-300/50"
                            aria-haspopup="menu"
                            x-bind:aria-expanded="open ? 'true' : 'false'"
                        >
                            <span class="flex size-8 items-center justify-center rounded-full bg-gradient-to-br from-primary to-secondary text-xs font-bold text-white shadow-sm">{{ $initials }}</span>
                            <span class="hidden text-sm font-medium text-base-content/80 sm:block">{{ $user->full_name }}</span>
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
                            {{-- Kullanıcı bilgileri --}}
                            <div class="flex items-center gap-3 border-b border-base-200 bg-gradient-to-r from-primary/[0.06] to-secondary/[0.04] px-4 py-3.5">
                                <span class="flex size-10 shrink-0 items-center justify-center rounded-full bg-gradient-to-br from-primary to-secondary text-sm font-bold text-white shadow-md shadow-primary/20">{{ $initials }}</span>
                                <div class="min-w-0">
                                    <p class="truncate text-sm font-semibold text-base-content">{{ $user->full_name }}</p>
                                    <p class="truncate text-xs text-base-content/50">{{ $user->email }}</p>
                                </div>
                            </div>

                            {{-- Rol --}}
                            <div class="px-4 py-2.5">
                                <span @class([
                                    'badge badge-sm font-medium',
                                    'badge-primary' => $user->isAdmin(),
                                    'badge-outline border-primary/40 text-primary' => $user->isTeacher(),
                                ])>{{ $roleLabel }}</span>
                            </div>

                            {{-- Menü --}}
                            <div class="border-t border-base-200 p-1.5">
                                <a href="{{ route('teacher.dashboard') }}" class="flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-base-content/75 transition hover:bg-primary/10 hover:text-primary">
                                    <x-icon name="heroicon-o-squares-2x2" class="size-4" />
                                    Müəllim Paneli
                                </a>
                            </div>

                            {{-- Logout --}}
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
                </div>
            </div>
        </header>

        {{-- İçerik --}}
        <main class="flex-1" @style(['width: 100%' => config('theme.full_bleed', true), 'max-width: 72rem; margin-inline: auto; width: 100%' => ! config('theme.full_bleed', true)])>
            <div @style([
                'padding-left: '.config('theme.side_padding', 40).'px',
                'padding-right: '.config('theme.side_padding', 40).'px',
            ]) class="relative">
                {{-- Dekorativ gradyan fon (login ambiansı) --}}
                <div aria-hidden="true" class="pointer-events-none absolute inset-x-0 top-0 h-72 bg-gradient-to-b from-primary/[0.07] via-secondary/[0.04] to-transparent"></div>

                <div class="relative py-6">
                    <x-teacher.flash />
                    @yield('content')
                </div>
            </div>
        </main>
    </div>

    @stack('scripts')
    @vite('resources/js/teacher/index.js')
</body>
</html>
