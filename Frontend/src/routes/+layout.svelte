<script>
    import '../app.css';
    import { onMount } from 'svelte';
    import { locale, t } from '$lib/i18n.js';
    import { loadUser, ready } from '$lib/auth.js';
    import { setNotifyProvider } from '$lib/notify.js';

    onMount(() => {
        if (typeof window.Swal === 'undefined') {
            import('sweetalert2').then((m) => {
                window.Swal = m.default;
            });
        }
        if (document.body.dataset.notifyProvider) {
            setNotifyProvider(document.body.dataset.notifyProvider);
        }
        loadUser();
    });
</script>

<div class="min-h-screen bg-base-100 text-base-content" data-theme="kela">
    {#if $ready}
        <slot />
    {:else}
        <div class="flex min-h-screen items-center justify-center">
            <span class="loading loading-spinner loading-lg text-primary"></span>
        </div>
    {/if}
</div>
