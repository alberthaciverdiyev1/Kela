<script>
    let { title = '', children, open = $bindable(false) } = $props();
    let dialog;

    $effect(() => {
        if (!dialog) return;
        if (open && !dialog.open) dialog.showModal();
        if (!open && dialog.open) dialog.close();
    });

    function close() {
        open = false;
        if (dialog && dialog.open) dialog.close();
    }
</script>

<dialog bind:this={dialog} class="modal" onclose={close}>
    <div class="modal-box">
        {#if title}
            <div class="flex items-center justify-between border-b border-base-200 px-6 py-4">
                <h2 class="text-lg font-semibold tracking-tight">{title}</h2>
                <button type="button" class="btn btn-sm btn-ghost btn-circle" onclick={close}>
                    <span class="text-base-content/50">✕</span>
                </button>
            </div>
        {/if}
        <div class="px-6 py-5">{@render children?.()}</div>
    </div>
    <form method="dialog" class="modal-backdrop">
        <button onclick={close}>close</button>
    </form>
</dialog>
