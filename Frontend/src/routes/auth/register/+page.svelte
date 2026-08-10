<script>
    import { goto } from '$app/navigation';
    import { api } from '$lib/api.js';
    import { t } from '$lib/i18n.js';
    import AppBtn from '$lib/components/AppBtn.svelte';
    import { notify } from '$lib/notify.js';

    let firstName = '';
    let lastName = '';
    let email = '';
    let password = '';
    let passwordConfirm = '';
    let error = '';
    let busy = false;

    async function submit(e) {
        e.preventDefault();
        error = '';
        if (!firstName) { error = $t('auth.reqFirstName'); return; }
        if (!email) { error = $t('auth.reqEmail'); return; }
        if (!password) { error = $t('auth.reqPassword'); return; }
        if (password.length < 6) { error = $t('auth.passwordMin'); return; }
        if (password !== passwordConfirm) { error = $t('auth.passwordMismatch'); return; }
        busy = true;
        try {
            await api('/auth/register', {
                method: 'POST',
                body: JSON.stringify({
                    firstName,
                    lastName: lastName || null,
                    email,
                    password
                })
            });
            notify.success($t('auth.registerSuccess'));
            goto('/auth/login');
        } catch (err) {
            error = err.errors && err.errors.length ? err.errors[0] : (err.message || $t('common.error'));
        } finally {
            busy = false;
        }
    }
</script>

<div class="flex min-h-screen items-center justify-center bg-base-200 p-4">
    <div class="card w-full max-w-sm border border-base-200 bg-base-100 shadow-xl">
        <div class="card-body gap-4">
            <div>
                <h1 class="text-xl font-semibold tracking-tight">{$t('auth.register')}</h1>
                <p class="text-sm text-base-content/50">{$t('auth.registerSubtitle')}</p>
            </div>

            {#if error}
                <div class="alert alert-error text-sm">{error}</div>
            {/if}

            <form onsubmit={submit} class="flex flex-col gap-4">
                <div class="grid grid-cols-2 gap-3">
                    <div class="field">
                        <label class="block text-sm font-semibold mb-1.5 text-base-content" for="reg-first">{$t('auth.firstName')}</label>
                        <input id="reg-first" class="input w-full" type="text" bind:value={firstName} required />
                    </div>
                    <div class="field">
                        <label class="block text-sm font-semibold mb-1.5 text-base-content" for="reg-last">{$t('auth.lastName')}</label>
                        <input id="reg-last" class="input w-full" type="text" bind:value={lastName} />
                    </div>
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="reg-email">{$t('auth.email')}</label>
                    <input id="reg-email" class="input w-full" type="email" bind:value={email} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="reg-pass">{$t('auth.password')}</label>
                    <input id="reg-pass" class="input w-full" type="password" bind:value={password} required />
                </div>
                <div class="field">
                    <label class="block text-sm font-semibold mb-1.5 text-base-content" for="reg-pass2">{$t('auth.passwordConfirm')}</label>
                    <input id="reg-pass2" class="input w-full" type="password" bind:value={passwordConfirm} required />
                </div>
                <AppBtn variant="primary" type="submit" loading={busy}>{$t('auth.register')}</AppBtn>
            </form>

            <p class="text-center text-sm text-base-content/50">
                {$t('auth.haveAccount')}
                <a class="link text-primary" href="/auth/login">{$t('auth.login')}</a>
            </p>
        </div>
    </div>
</div>
