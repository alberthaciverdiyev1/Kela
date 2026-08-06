<template>
  <div class="space-y-6">
    <Card class="border-none shadow-lg rounded-2xl overflow-hidden">
      <template #content>
        <div class="bg-gradient-to-r from-primary to-primary-400 p-6 -m-6 mb-0 text-white">
          <div class="flex items-center gap-4">
            <Avatar :label="initials" size="xlarge" shape="circle" class="bg-white/20 text-white" />
            <div>
              <h1 class="text-2xl font-bold m-0">{{ i18n.t('panel.welcome', { name: auth.fullName }) }}</h1>
              <div class="flex items-center gap-2 mt-2">
                <Chip :label="auth.roleName" class="bg-white/20 text-white border-none" />
              </div>
            </div>
          </div>
        </div>
      </template>
    </Card>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <StatCard icon="pi pi-users" icon-bg-class="bg-primary-50 text-primary" :label="i18n.t('nav.classes')" :value="0" />
      <StatCard icon="pi pi-user" icon-bg-class="bg-success-50 text-success-600" :label="i18n.t('nav.students')" :value="0" />
      <StatCard icon="pi pi-file-edit" icon-bg-class="bg-warning-50 text-warning-600" :label="i18n.t('nav.homework')" :value="0" />
    </div>

    <Card class="border-none shadow rounded-2xl">
      <template #content>
        <Message severity="info" :closable="false">
          {{ i18n.t('teacher.info') }}
        </Message>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore } from '../../stores/auth'
import { useI18n } from '../../stores/i18n'
import StatCard from '../../components/ui/cards/StatCard.vue'

const auth = useAuthStore()
const i18n = useI18n()

const initials = computed(() => {
  const name = auth.fullName.trim()
  if (!name) return '?'
  const parts = name.split(' ')
  return (parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')
})
</script>
