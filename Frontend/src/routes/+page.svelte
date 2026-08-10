<script>
    import { onMount } from 'svelte';
    import { goto } from '$app/navigation';
    import { user, ready, homeFor } from '$lib/auth.js';

    onMount(() => {
        if (!$ready) return;
        if (!$user) goto('/auth/login');
        else goto(homeFor($user.role));
    });
</script>

<div class="flex min-h-screen items-center justify-center">
    <span class="loading loading-spinner loading-lg text-primary"></span>
</div>
