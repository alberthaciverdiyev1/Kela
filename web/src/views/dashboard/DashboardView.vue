<template>
  <div class="space-y-6">
    <!-- Karşılama bandı -->
    <Card class="border-none shadow-lg rounded-2xl overflow-hidden">
      <template #content>
        <div class="bg-gradient-to-r from-primary to-primary-400 p-6 -m-6 mb-0 text-white">
          <div class="flex items-center gap-4">
            <Avatar :label="initials" size="xlarge" shape="circle" class="bg-white/20 text-white" />
            <div>
              <h1 class="text-2xl font-bold m-0">Hoş geldin, {{ auth.fullName }}!</h1>
              <div class="flex items-center gap-2 mt-2">
                <Chip :label="roleName" class="bg-white/20 text-white border-none" />
              </div>
            </div>
          </div>
        </div>
      </template>
    </Card>

    <!-- Stat kartları -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <Card class="border-none shadow hover:shadow-lg transition-shadow rounded-2xl">
        <template #content>
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 rounded-xl bg-primary-50 text-primary flex items-center justify-center">
              <i class="pi pi-book text-2xl"></i>
            </div>
            <div>
              <div class="text-2xl font-bold">0</div>
              <div class="text-sm text-surface-500">Kurslar</div>
            </div>
          </div>
        </template>
      </Card>

      <Card class="border-none shadow hover:shadow-lg transition-shadow rounded-2xl">
        <template #content>
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 rounded-xl bg-success-50 text-success-600 flex items-center justify-center">
              <i class="pi pi-users text-2xl"></i>
            </div>
            <div>
              <div class="text-2xl font-bold">0</div>
              <div class="text-sm text-surface-500">Öğrenciler</div>
            </div>
          </div>
        </template>
      </Card>

      <Card class="border-none shadow hover:shadow-lg transition-shadow rounded-2xl">
        <template #content>
          <div class="flex items-center gap-4">
            <div class="w-12 h-12 rounded-xl bg-warning-50 text-warning-600 flex items-center justify-center">
              <i class="pi pi-file-edit text-2xl"></i>
            </div>
            <div>
              <div class="text-2xl font-bold">0</div>
              <div class="text-sm text-surface-500">Ödevler</div>
            </div>
          </div>
        </template>
      </Card>
    </div>

    <Card class="border-none shadow rounded-2xl">
      <template #content>
        <Message severity="info" :closable="false">
          Panel özellikleri yakında eklenecek. Şu an auth akışı çalışıyor: giriş, kayıt ve çıkış.
        </Message>
      </template>
    </Card>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useAuthStore, ROLES } from '../../stores/auth'

const auth = useAuthStore()

const initials = computed(() => {
  const name = auth.fullName.trim()
  if (!name) return '?'
  const parts = name.split(' ')
  return (parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')
})

const roleName = {
  [ROLES.Admin]: 'Admin',
  [ROLES.Teacher]: 'Teacher',
  [ROLES.Student]: 'Student',
  [ROLES.Parent]: 'Parent',
}[auth.role] ?? 'Bilinmiyor'
</script>
