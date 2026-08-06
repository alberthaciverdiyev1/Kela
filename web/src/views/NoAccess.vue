<template>
  <div class="min-h-screen flex items-center justify-center bg-surface-50 p-6">
    <Card class="max-w-md w-full border-none shadow-lg rounded-2xl">
      <template #content>
        <div class="text-center py-4">
          <div class="w-16 h-16 mx-auto rounded-full bg-warning-100 text-warning-600 flex items-center justify-center mb-4">
            <i class="pi pi-lock text-2xl"></i>
          </div>
          <h1 class="text-xl font-bold text-surface-800 mb-2">{{ i18n.t('noAccess.title') }}</h1>
          <p class="text-surface-500 mb-1">{{ i18n.t('noAccess.desc1') }}</p>
          <p class="text-surface-500 mb-6">{{ i18n.t('noAccess.desc2') }}</p>
          <Button :label="i18n.t('nav.logout')" icon="pi pi-sign-out" @click="auth.logout()" />
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { useAuthStore } from '../stores/auth'
import { useI18n } from '../stores/i18n'

const auth = useAuthStore()
const i18n = useI18n()
</script>
