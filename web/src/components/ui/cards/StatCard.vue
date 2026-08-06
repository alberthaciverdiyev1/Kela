<template>
  <Card class="border-none shadow hover:shadow-lg transition-shadow rounded-2xl">
    <template #content>
      <div class="flex items-center gap-4">
        <div :class="['w-12 h-12 rounded-xl flex items-center justify-center', iconBgClass]">
          <i :class="['pi', icon, 'text-2xl']"></i>
        </div>
        <div>
          <div class="text-2xl font-bold">{{ value }}</div>
          <div class="text-sm text-surface-500">{{ label }}</div>
        </div>
      </div>
    </template>
  </Card>
</template>

<script setup>
defineProps({
  icon: { type: String, required: true },
  iconBgClass: { type: String, required: true },
  value: { type: [String, Number], default: 0 },
  label: { type: String, required: true },
})
</script>
