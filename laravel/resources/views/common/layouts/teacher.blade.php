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
        $navGroups = [
            [
                'type' => 'link',
                'label' => 'Müəllim Paneli',
                'icon' => 'home',
                'url' => route('teacher.dashboard'),
                'active' => $path === 'teacher/dashboard'
            ],
            [
                'type' => 'link',
                'label' => 'Şagirdlər',
                'icon' => 'users',
                'url' => route('teacher.students.index'),
                'active' => str_starts_with($path, 'teacher/students')
            ],
            [
                'type' => 'group',
                'label' => 'Tədris',
                'icon' => 'academic-cap',
                'items' => [
                    ['label' => 'Dərslər', 'url' => route('teacher.lessons.index'), 'active' => str_starts_with($path, 'teacher/lessons'), 'icon' => 'academic-cap'],
                    ['label' => 'Quizlər', 'url' => route('teacher.quizzes.index'), 'active' => str_starts_with($path, 'teacher/quizzes'), 'icon' => 'clipboard-document-list'],
                    ['label' => 'Ev Tapşırığı', 'url' => route('teacher.homeworks.index'), 'active' => str_starts_with($path, 'teacher/homeworks'), 'icon' => 'book-open'],
                    ['label' => 'Sual Bankı', 'url' => route('teacher.questions.index'), 'active' => str_starts_with($path, 'teacher/questions'), 'icon' => 'queue-list'],
                    ['label' => 'Davam', 'url' => route('teacher.attendance.index'), 'active' => str_starts_with($path, 'teacher/attendance'), 'icon' => 'calendar-days'],
                ]
            ],
            [
                'type' => 'link',
                'label' => 'Siniflər',
                'icon' => 'building-office-2',
                'url' => route('teacher.workspaces.index'),
                'active' => str_starts_with($path, 'teacher/workspaces')
            ],
            [
                'type' => 'link',
                'label' => 'Qeydlər',
                'icon' => 'pencil-square',
                'url' => route('teacher.notes.index'),
                'active' => str_starts_with($path, 'teacher/notes')
            ],
            [
                'type' => 'link',
                'label' => 'Ödənişlər',
                'icon' => 'banknotes',
                'url' => route('teacher.payments.index'),
                'active' => str_starts_with($path, 'teacher/payments')
            ]
        ];
        $roleLabel = match (true) {
            $user->isAdmin() => 'Admin',
            $user->isTeacher() => 'Müəllim',
            default => 'İstifadəçi',
        };
        $initials = strtoupper(mb_substr($user->first_name, 0, 1).mb_substr($user->last_name, 0, 1));
    @endphp

    <div class="flex min-h-screen flex-col">
        {{-- Üst aksent xətti --}}
        <div class="h-0.5 w-full bg-primary"></div>

        {{-- Teacher panel üst navbar --}}
        <header class="sticky top-0 z-40 border-b border-base-300/80 bg-base-100/85 shadow-sm backdrop-blur-lg">
            <div class="grid h-16 grid-cols-[auto_1fr_auto] items-center gap-4 px-4 sm:px-6 lg:px-10">
                {{-- Logo / marka --}}
                <a href="{{ route('teacher.dashboard') }}" class="flex shrink-0 items-center gap-2.5">
                    <span class="flex size-9 items-center justify-center rounded-lg bg-primary text-base font-bold text-white">K</span>
                    <span class="text-xl font-bold tracking-tight text-base-content">Kela</span>
                </a>

                {{-- Navigasiya --}}
                <nav class="flex items-center justify-center gap-1.5 text-sm font-medium">
                    @foreach($navGroups as $group)
                        @if(isset($group['type']) && $group['type'] === 'link')
                            <a href="{{ $group['url'] }}" 
                                @class([
                                    'group flex items-center gap-1.5 rounded-xl px-3 py-2 transition-all duration-200',
                                    'bg-primary/10 text-primary' => $group['active'],
                                    'text-base-content/65 hover:bg-base-300/50 hover:text-base-content' => !$group['active'],
                                ])>
                                <x-icon name="heroicon-o-{{ $group['icon'] }}" class="size-4 opacity-70" />
                                {{ $group['label'] }}
                            </a>
                        @else
                            @php $isGroupActive = collect($group['items'])->contains('active', true); @endphp
                            <div class="relative" x-data="{ open: false }" @click.outside="open = false" @keydown.escape.window="open = false">
                                <button @click="open = !open" 
                                    @class([
                                        'group flex items-center gap-1.5 rounded-xl px-3 py-2 transition-all duration-200',
                                        'bg-primary/10 text-primary' => $isGroupActive,
                                        'text-base-content/65 hover:bg-base-300/50 hover:text-base-content' => !$isGroupActive,
                                    ])>
                                    <x-icon name="heroicon-o-{{ $group['icon'] }}" class="size-4 opacity-70" />
                                    {{ $group['label'] }}
                                    <x-icon name="heroicon-o-chevron-down" class="size-3.5 opacity-60 transition-transform" x-bind:class="open ? 'rotate-180' : ''" />
                                </button>
                                
                                <div x-cloak x-show="open" x-transition.opacity.duration.150ms class="absolute left-0 top-full mt-1.5 w-48 rounded-xl border border-base-200 bg-base-100 p-1.5 shadow-lg">
                                    @foreach($group['items'] as $item)
                                        <a href="{{ $item['url'] }}" @class([
                                            'flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm transition',
                                            'bg-primary/10 text-primary font-semibold' => $item['active'],
                                            'text-base-content/80 hover:bg-base-200 hover:text-base-content' => !$item['active']
                                        ])>
                                            <x-icon name="heroicon-o-{{ $item['icon'] }}" class="size-4 opacity-70" />
                                            {{ $item['label'] }}
                                        </a>
                                    @endforeach
                                </div>
                            </div>
                        @endif
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
                            @if($user->avatar_url)
                                <img src="{{ $user->avatar_url }}" alt="{{ $user->full_name }}" class="size-8 rounded-full object-cover ring-2 ring-primary/20" />
                            @else
                                <span class="flex size-8 items-center justify-center rounded-full bg-primary text-xs font-bold text-white">{{ $initials }}</span>
                            @endif
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
                            <div class="flex items-center gap-3 border-b border-base-200 bg-base-200/40 px-4 py-3.5">
                                @if($user->avatar_url)
                                    <img src="{{ $user->avatar_url }}" alt="{{ $user->full_name }}" class="size-10 shrink-0 rounded-full object-cover ring-2 ring-primary/20" />
                                @else
                                    <span class="flex size-10 shrink-0 items-center justify-center rounded-full bg-primary text-sm font-bold text-white">{{ $initials }}</span>
                                @endif
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
                                    <x-icon name="heroicon-o-squares-2x2" class="size-4 opacity-70" />
                                    Müəllim Paneli
                                </a>
                                <a href="{{ route('profile.edit') }}" class="flex items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium text-base-content/75 transition hover:bg-primary/10 hover:text-primary">
                                    <x-icon name="heroicon-o-user" class="size-4 opacity-70" />
                                    Profil Ayarları
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
