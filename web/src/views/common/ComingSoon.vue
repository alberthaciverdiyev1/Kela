<template>
  <Card class="max-w-2xl mx-auto border-none shadow rounded-2xl">
    <template #content>
      <div class="text-center py-10">
        <div class="w-16 h-16 mx-auto rounded-full bg-primary-50 text-primary flex items-center justify-center mb-4">
          <i class="pi pi-sparkles text-2xl"></i>
        </div>
        <h1 class="text-xl font-bold text-surface-800 mb-2">{{ pageTitle }}</h1>
        <p class="text-surface-500 mb-6">{{ i18n.t('comingSoon.desc') }}</p>
        <Button :label="i18n.t('comingSoon.back')" icon="pi pi-arrow-left" @click="router.push({ name: homeRouteFor(auth.role) })" />
      </div>
    </template>
  </Card>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore, homeRouteFor } from '../../stores/auth'
import { useI18n } from '../../stores/i18n'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const i18n = useI18n()

const pageTitle = computed(() => i18n.t(route.meta.title ?? 'comingSoon.title'))
</script>
