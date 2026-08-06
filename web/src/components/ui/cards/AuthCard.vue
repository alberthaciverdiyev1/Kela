<template>
  <Card class="border-none shadow-2xl rounded-3xl overflow-hidden">
    <template #content>
      <!-- Üst bant -->
      <div class="bg-primary text-white text-center py-8 rounded-3xl rounded-b-none -mt-6 -mx-6 mb-6">
        <div class="mx-auto w-16 h-16 bg-white/20 rounded-2xl flex items-center justify-center mb-3">
          <i :class="['pi', icon, 'text-3xl']"></i>
        </div>
        <h1 class="text-2xl font-bold m-0">Kela LMS</h1>
        <p class="text-white/80 m-0 mt-1 text-sm">{{ subtitle }}</p>
      </div>

      <slot name="messages" />
      <slot />

      <Divider class="my-6">
        <span class="text-sm text-surface-400">veya</span>
      </Divider>
      <div class="text-center text-sm">
        <span class="text-surface-500">{{ footerPrompt }}</span>
        <router-link :to="{ name: footerLinkRoute }" class="text-primary font-semibold ml-1">
          {{ footerLinkText }}
        </router-link>
      </div>
    </template>
  </Card>
</template>

<script setup>
defineProps({
  icon: { type: String, required: true },
  subtitle: { type: String, required: true },
  footerPrompt: { type: String, required: true },
  footerLinkText: { type: String, required: true },
  footerLinkRoute: { type: String, required: true },
})
</script>
