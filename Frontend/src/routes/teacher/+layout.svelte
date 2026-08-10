<script>
    import { onMount } from 'svelte';
    import { goto } from '$app/navigation';
    import { user, ready, logout } from '$lib/auth.js';
    import { t, locale, setLocale } from '$lib/i18n.js';
    import { http } from '$lib/api.js';
    import AppIcon from '$lib/components/AppIcon.svelte';

    const NAV = [
        { href: '/teacher/dashboard', icon: 'dashboard', key: 'nav.dashboard' },
        { href: '/teacher/library', icon: 'book', key: 'nav.library' },
        { href: '/teacher/questions', icon: 'quiz', key: 'questions.title' },
        { href: '/teacher/students', icon: 'students', key: 'nav.students' },
        { href: '/teacher/workspaces', icon: 'classes', key: 'nav.workspaces' },
        { href: '/teacher/attendance', icon: 'calendar-check', key: 'nav.attendance' }
    ];

    function changeLang(e) {
        setLocale(e.target.value);
    }

    onMount(() => {
        if (!$ready) return;
        if (!$user) goto('/auth/login');
        else if (!['Teacher', 'Admin'].includes($user.role)) goto('/blocked');
    });

    async function doLogout() {
        try {
            await http.post('/auth/logout');
        } catch (e) {}
        logout();
        goto('/auth/login');
    }
</script>

{#if $ready && $user && ['Teacher', 'Admin'].includes($user.role)}
    <header class="sticky top-0 z-40 border-b border-base-200 bg-base-100/80 backdrop-blur">
        <div class="mx-auto flex h-14 max-w-7xl items-center justify-between gap-4 px-4">
            <a href="/teacher/dashboard" class="flex items-center gap-2 font-bold tracking-tight">
                <span class="flex h-7 w-7 items-center justify-center rounded-lg bg-primary text-sm text-primary-content">K</span>
                <span>Kela LMS</span>
            </a>
            <nav class="hidden items-center gap-1 md:flex">
                {#each NAV as item}
                    <a href={item.href} class="btn btn-ghost btn-sm">
                        <AppIcon name={item.icon} class="w-4 h-4" />
                        <span>{$t(item.key)}</span>
                    </a>
                {/each}
            </nav>
            <div class="flex items-center gap-2">
                <select class="select select-sm" value={$locale} onchange={changeLang}>
                    <option value="az">AZ</option>
                    <option value="tr">TR</option>
                    <option value="en">EN</option>
                    <option value="ru">RU</option>
                </select>
                <span class="hidden text-sm text-base-content/60 sm:inline">{$user.firstName} {$user.lastName}</span>
                <button type="button" class="btn btn-ghost btn-sm btn-square" title={$t('nav.logout')} onclick={doLogout}>
                    <AppIcon name="logout" class="w-4 h-4" />
                </button>
            </div>
        </div>
    </header>

    <main class="mx-auto max-w-7xl px-4 py-6">
        <slot />
    </main>
{:else}
    <div class="flex min-h-screen items-center justify-center">
        <span class="loading loading-spinner loading-lg text-primary"></span>
    </div>
{/if}
