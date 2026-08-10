<script>
    import AppIcon from '$lib/components/AppIcon.svelte';

    let { title, subtitle = null, icon = null, children } = $props();
</script>

<div class="flex flex-wrap items-center justify-between gap-3">
    <div class="flex items-center gap-3">
        {#if icon}
            <div class="flex h-10 w-10 items-center justify-center rounded-xl bg-primary/10 text-primary">
                <AppIcon name={icon} class="w-5 h-5" />
            </div>
        {/if}
        <div>
            <h1 class="text-xl font-semibold tracking-tight text-base-content">{title}</h1>
            {#if subtitle}<p class="text-sm text-base-content/50">{subtitle}</p>{/if}
        </div>
    </div>
    {#if children}<div class="flex gap-2">{@render children?.()}</div>{/if}
</div>
