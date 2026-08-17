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
            <div class="mx-auto grid h-16 max-w-screen-2xl grid-cols-[auto_1fr_auto] items-center gap-4 px-4 sm:px-6">
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

                    <span @class([
                        'badge badge-sm font-medium',
                        'badge-primary' => $user->isAdmin(),
                        'badge-outline border-primary/40 text-primary' => $user->isTeacher(),
                    ])>{{ $roleLabel }}</span>

                    <span class="hidden items-center gap-2 md:flex">
                        <span class="flex size-8 items-center justify-center rounded-full bg-gradient-to-br from-primary to-secondary text-xs font-bold text-white shadow-sm">{{ $initials }}</span>
                        <span class="text-sm font-medium text-base-content/80">{{ $user->full_name }}</span>
                    </span>

                    <form method="POST" action="{{ route('auth.logout') }}">
                        @csrf
                        <button type="submit" class="btn btn-ghost btn-sm gap-1.5 text-base-content/70 hover:bg-error/10 hover:text-error">
                            <x-icon name="heroicon-o-arrow-right-start-on-rectangle" class="size-4" />
                            <span class="hidden sm:inline">Çıxış</span>
                        </button>
                    </form>
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
