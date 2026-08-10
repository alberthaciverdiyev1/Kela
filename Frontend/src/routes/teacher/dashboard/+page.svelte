<script>
    import { t } from '$lib/i18n.js';
    import { user } from '$lib/auth.js';
    import AppIcon from '$lib/components/AppIcon.svelte';
    import AppPageHead from '$lib/components/AppPageHead.svelte';

    const name = () => ($user ? $user.firstName + ' ' + $user.lastName : '').trim();
    const initials = () => {
        const parts = name().split(/\s+/).filter(Boolean);
        return ((parts[0] || '?')[0] || '') + ((parts[1] || '')[0] || '');
    };

    const STATS = [
        { icon: 'folder', labelKey: 'nav.workspaces', value: 0 },
        { icon: 'students', tone: 'success', labelKey: 'nav.students', value: 0 },
        { icon: 'book', tone: 'warning', labelKey: 'nav.homework', value: 0 }
    ];
</script>

<AppPageHead icon="dashboard">
    <div class="flex items-center gap-3">
        <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-primary text-sm font-bold text-primary-content">{initials()}</div>
        <div>
            <h1 class="text-xl font-semibold tracking-tight">{$t('panel.welcome', { name: name() })}</h1>
            <p class="text-sm text-base-content/50">{$t('role.Teacher')}</p>
        </div>
    </div>
</AppPageHead>

<div class="mt-6 grid gap-4 sm:grid-cols-3">
    {#each STATS as s}
        <div class="card border border-base-200 bg-base-100">
            <div class="card-body flex-row items-center gap-3 py-4">
                <span class="text-primary/60"><AppIcon name={s.icon} class="w-6 h-6" /></span>
                <div>
                    <p class="text-2xl font-bold leading-none">{s.value}</p>
                    <p class="text-sm text-base-content/50">{$t(s.labelKey)}</p>
                </div>
            </div>
        </div>
    {/each}
</div>

<div class="mt-6 card border border-base-200 bg-base-100">
    <div class="card-body">
        <div class="alert alert-info text-sm">{$t('teacher.info')}</div>
    </div>
</div>
