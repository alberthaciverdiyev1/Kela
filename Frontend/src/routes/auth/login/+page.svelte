<script>
    import { onMount } from 'svelte';
    import { goto } from '$app/navigation';
    import { login, user, ready, homeFor } from '$lib/auth.js';
    import { t } from '$lib/i18n.js';
    import AppBtn from '$lib/components/AppBtn.svelte';

    let email = '';
    let password = '';
    let error = '';
    let busy = false;

    onMount(() => {
        if ($ready && $user) goto(homeFor($user.role));
    });

    async function submit(e) {
        e.preventDefault();
        error = '';
        if (!email || !password) {
            error = $t('auth.reqEmail') || $t('auth.reqPassword');
            return;
        }
        busy = true;
        try {
            const me = await login(email, password);
            goto(homeFor(me.role));
        } catch (err) {
            error = err.message || $t('common.error');
        } finally {
            busy = false;
        }
    }
</script>

<div class="flex min-h-screen items-center justify-center bg-base-200 p-4">
    <div class="card w-full max-w-sm border border-base-200 bg-base-100 shadow-xl">
        <div class="card-body gap-4">
            <div class="flex items-center gap-3">
                <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-primary/10 text-xl font-bold text-primary">K</div>
                <div>
                    <h1 class="text-xl font-semibold tracking-tight">Kela LMS</h1>
                    <p class="text-sm text-base-content/50">{$t('auth.loginSubtitle')}</p>
                </div>
            </div>

            {#if error}
                <div class="alert alert-error text-sm">{$t('common.error')}: {error}</div>
            {/if}

            <form onsubmit={submit} class="flex flex-col gap-4">
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="login-email">{$t('auth.email')}</label>
                    <input id="login-email" class="input w-full" type="email" bind:value={email} required autocomplete="email" />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="login-password">{$t('auth.password')}</label>
                    <input id="login-password" class="input w-full" type="password" bind:value={password} required autocomplete="current-password" />
                </div>
                <AppBtn variant="primary" type="submit" loading={busy}>{$t('auth.login')}</AppBtn>
            </form>

            <p class="text-center text-sm text-base-content/50">
                {$t('auth.noAccount')}
                <a class="link text-primary" href="/auth/register">{$t('auth.register')}</a>
            </p>
        </div>
    </div>
</div>
