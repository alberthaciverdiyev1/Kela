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
    <header class="navbar bg-base-100 border-b border-base-300 shadow-sm">
        <div class="flex items-center gap-4 px-4">
            <span class="text-lg font-bold tracking-tight">{{ $siteConfig?->site_name ?? config('app.name') }}</span>
            @auth
                <nav class="flex gap-2 text-sm">
                    @if(auth()->user()->isAdmin())
                        <a href="{{ route('teacher.dashboard') }}" class="link link-hover">Panel</a>
                        <a href="#" class="link link-hover">Dərslər</a>
                        <a href="#" class="link link-hover">Sual Bankası</a>
                    @elseif(auth()->user()->isTeacher())
                        <a href="{{ route('teacher.dashboard') }}" class="link link-hover">Panel</a>
                        <a href="#" class="link link-hover">Dərslər</a>
                        <a href="#" class="link link-hover">Quizlər</a>
                        <a href="#" class="link link-hover">İş Sahələri</a>
                        <a href="#" class="link link-hover">Yoklama</a>
                    @elseif(auth()->user()->isStudent())
                        <a href="{{ route('student.dashboard') }}" class="link link-hover">Dərslər</a>
                    @endif
                </nav>
            @endauth
            <div class="ms-auto flex items-center gap-2">
                @auth
                    <span class="text-sm text-base-content/70">{{ auth()->user()->full_name }}</span>
                    <form method="POST" action="{{ route('auth.logout') }}">
                        @csrf
                        <button class="btn btn-sm btn-ghost" type="submit">Çıxış</button>
                    </form>
                @endauth
            </div>
        </div>
    </header>
    <main class="mx-auto max-w-6xl p-6">
        @if(session('status'))
            <div class="alert alert-success mb-4">{{ session('status') }}</div>
        @endif
        @yield('content')
    </main>
</body>
</html>
