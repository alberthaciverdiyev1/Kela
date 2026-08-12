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
        $nav[] = ['label' => 'Müəllim Paneli', 'url' => route('teacher.dashboard'), 'active' => $path === 'teacher/dashboard'];
        $nav[] = ['label' => 'Şagirdlər', 'url' => route('teacher.students.index'), 'active' => str_starts_with($path, 'teacher/students')];
        $nav[] = ['label' => 'Sual Bankı', 'url' => route('teacher.questions.index'), 'active' => str_starts_with($path, 'teacher/questions')];
        $nav[] = ['label' => 'İş Sahələri', 'url' => route('teacher.workspaces.index'), 'active' => str_starts_with($path, 'teacher/workspaces')];
        $nav[] = ['label' => 'Davam', 'url' => route('teacher.attendance.index'), 'active' => str_starts_with($path, 'teacher/attendance')];
        $nav[] = ['label' => 'Dərslər', 'url' => route('teacher.lessons.index'), 'active' => str_starts_with($path, 'teacher/lessons')];
        $nav[] = ['label' => 'Quizlər', 'url' => route('teacher.quizzes.index'), 'active' => str_starts_with($path, 'teacher/quizzes')];
        $roleLabel = match (true) {
            $user->isAdmin() => 'Admin',
            $user->isTeacher() => 'Müəllim',
            default => 'İstifadəçi',
        };
    @endphp

    <div class="flex min-h-screen flex-col">
        {{-- Teacher panel üst navbar --}}
        <header class="sticky top-0 z-40 border-b border-base-300 bg-base-100 shadow-sm">
            <div class="grid h-16 grid-cols-[auto_1fr_auto] items-center gap-4 px-6">
                <a href="/teacher/dashboard" class="text-lg font-bold tracking-tight text-base-content">Kela</a>
                <nav class="flex items-center justify-center gap-1 text-sm font-medium">
                    @foreach($nav as $item)
                        <a href="{{ $item['url'] }}"
                           @class([
                               'rounded-lg px-3 py-1.5 transition',
                               'bg-primary/10 text-primary' => $item['active'],
                               'text-base-content/70 hover:bg-base-300/60 hover:text-base-content' => ! $item['active'],
                           ])>
                            {{ $item['label'] }}
                        </a>
                    @endforeach
                </nav>
                <div class="flex items-center justify-end gap-3">
                    <button
                        type="button"
                        x-data="{ dark: document.documentElement.getAttribute('data-theme') === 'filament-dark' }"
                        @click="
                            dark = !dark;
                            document.documentElement.setAttribute('data-theme', dark ? 'filament-dark' : 'filament');
                            localStorage.setItem('kela-theme', dark ? 'filament-dark' : 'filament');
                        "
                        class="btn btn-ghost btn-circle btn-sm"
                        title="Açıq / tünd rejim"
                        aria-label="Tema dəyişdir"
                    >
                        <x-icon name="heroicon-o-moon" class="size-4" x-show="!dark" />
                        <x-icon name="heroicon-o-sun" class="size-4" x-show="dark" />
                    </button>
                    <span class="badge badge-outline badge-sm">{{ $roleLabel }}</span>
                    <span class="text-sm text-base-content/80">{{ $user->full_name }}</span>
                    <form method="POST" action="{{ route('auth.logout') }}">
                        @csrf
                        <button type="submit" class="btn btn-ghost btn-sm">Çıxış</button>
                    </form>
                </div>
            </div>
        </header>

        {{-- İçerik --}}
        <main class="flex-1" @style(['width: 100%' => config('theme.full_bleed', true), 'max-width: 72rem; margin-inline: auto; width: 100%' => ! config('theme.full_bleed', true)])>
            <div @style([
                'padding-left: '.config('theme.side_padding', 40).'px',
                'padding-right: '.config('theme.side_padding', 40).'px',
            ]) class="py-6">
                <x-teacher.flash />
                @yield('content')
            </div>
        </main>
    </div>

    @stack('scripts')
    @vite('resources/js/teacher/index.js')
</body>
</html>
