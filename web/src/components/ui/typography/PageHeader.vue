<template>
  <div class="mb-6">
    <h1 class="text-2xl font-bold text-surface-800 mb-1">{{ title }}</h1>
    <p class="text-surface-500">{{ subtitle }}</p>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
})
</script>
