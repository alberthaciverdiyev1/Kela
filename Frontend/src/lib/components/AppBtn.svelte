<script>
    import AppIcon from '$lib/components/AppIcon.svelte';

    let {
        variant = 'primary',
        icon = null,
        href = null,
        type = 'button',
        disabled = false,
        loading = false,
        children,
        ...rest
    } = $props();

    const cls = () => {
        const base = 'btn btn-sm gap-2';
        const map = {
            primary: 'btn-primary',
            secondary: 'btn-secondary',
            ghost: 'btn-ghost',
            error: 'btn-error'
        };
        return base + ' ' + (map[variant] || 'btn-primary');
    };
</script>

{#if href}
    <a href={href} class={cls()} {...rest}>
        {#if icon}<AppIcon name={icon} class="w-4 h-4" />{/if}
        {@render children?.()}
    </a>
{:else}
    <button type={type} class={cls()} disabled={disabled || loading} {...rest}>
        {#if loading}<span class="loading loading-spinner loading-xs"></span>
        {:else if icon}<AppIcon name={icon} class="w-4 h-4" />{/if}
        {@render children?.()}
    </button>
{/if}
