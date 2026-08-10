<script>
    import { t } from '$lib/i18n.js';
    import { http } from '$lib/api.js';
    import AppIcon from '$lib/components/AppIcon.svelte';
    import AppBtn from '$lib/components/AppBtn.svelte';

    async function doLogout() {
        try {
            await http.post('/auth/logout');
        } catch (e) {}
        window.location.href = '/auth/login';
    }
</script>

<div class="flex min-h-screen items-center justify-center bg-base-200 p-4">
    <div class="card w-full max-w-md border border-base-200 bg-base-100">
        <div class="card-body items-center gap-3 text-center">
            <span class="text-warning/60"><AppIcon name="lock" class="w-8 h-8" /></span>
            <h1 class="text-xl font-semibold tracking-tight">{$t('noAccess.title')}</h1>
            <p class="text-base-content/60">{$t('noAccess.desc1')}</p>
            <p class="text-base-content/60">{$t('noAccess.desc2')}</p>
            <AppBtn variant="primary" icon="logout" onclick={doLogout}>{$t('nav.logout')}</AppBtn>
        </div>
    </div>
</div>
