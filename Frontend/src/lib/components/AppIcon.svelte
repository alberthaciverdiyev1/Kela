<script>
    import ICONS from '$lib/icons.js';

    let { name = 'students', class: cls = '' } = $props();
</script>

<svg class={cls} viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" width="20" height="20">
    {@html ICONS[name] || ICONS['students']}
</svg>
