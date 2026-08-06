<template>
  <Select
    :model-value="i18n.locale"
    :options="LANG_OPTIONS"
    option-label="label"
    option-value="code"
    class="lang-switcher"
    :pt="{ root: { style: 'min-width: 9.5rem' } }"
    @update:model-value="i18n.setLocale($event)"
  />
</template>

<script setup>
import { useI18n, LANG_OPTIONS } from '../stores/i18n'

const i18n = useI18n()
</script>

<style scoped>
/* Navbar/sidebar içinde kompakt görünüm */
.lang-switcher :deep(.p-select-label) {
  font-size: 0.875rem;
}
</style>
