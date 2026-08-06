<template>
  <Button
    type="submit"
    :label="label"
    :icon="icon"
    icon-pos="right"
    class="w-full justify-center py-3"
    :loading="loading"
    :disabled="disabled"
  />
</template>

<script setup>
defineProps({
  label: { type: String, required: true },
  icon: { type: String, default: undefined },
  loading: { type: Boolean, default: false },
  disabled: { type: Boolean, default: false },
})
</script>
