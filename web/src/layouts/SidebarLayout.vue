<template>
  <div class="min-h-screen flex bg-surface-50">
    <!-- Sol sidebar -->
    <aside class="w-64 shrink-0 bg-surface-0 border-r border-surface-200 flex flex-col">
      <!-- Logo -->
      <div class="flex items-center gap-3 px-5 h-16 border-b border-surface-100">
        <Avatar icon="pi pi-book" shape="circle" class="bg-primary text-white" />
        <span class="font-bold text-xl text-primary">{{ config.siteName }}</span>
      </div>

      <!-- Navigasyon -->
      <nav class="flex-1 p-4 space-y-1 overflow-y-auto">
        <router-link
          v-for="item in navItems"
          :key="item.name"
          :to="{ name: item.name }"
          class="flex items-center gap-3 px-4 py-2.5 rounded-lg text-surface-600 transition-colors"
          :class="isActive(item.name)
            ? 'bg-primary-50 text-primary font-semibold'
            : 'hover:bg-surface-100 hover:text-primary'"
        >
          <i :class="['pi', item.icon]"></i>
          <span>{{ item.label }}</span>
        </router-link>
      </nav>

      <!-- Kullanıcı -->
      <div class="p-4 border-t border-surface-100">
        <Menu ref="menuRef" :model="userMenuItems" popup class="p-2" />
        <div
          class="flex items-center gap-3 p-2 rounded-lg hover:bg-surface-100 cursor-pointer"
          @click="toggleMenu"
        >
          <Avatar :label="initials" shape="circle" class="bg-primary text-white" />
          <div class="flex-1 min-w-0">
            <div class="font-medium text-sm truncate">{{ auth.fullName }}</div>
            <div class="text-xs text-surface-400">{{ auth.roleName }}</div>
          </div>
          <i class="pi pi-chevron-up text-xs text-surface-400"></i>
        </div>
      </div>
    </aside>

    <!-- İçerik -->
    <div class="flex-1 flex flex-col min-w-0">
      <main class="flex-1 p-6">
        <router-view />
      </main>

      <footer class="text-center text-sm text-surface-400 py-4">
        © {{ new Date().getFullYear() }} {{ config.siteName }}
      </footer>
    </div>
  </div>
</template>

<script setup>
import { useRoute } from 'vue-router'
import { useUserMenu } from '../composables/useUserMenu'
import { useSiteConfigStore } from '../stores/siteConfig'

const route = useRoute()
const config = useSiteConfigStore()
const { auth, initials, userMenuItems, menuRef, toggleMenu } = useUserMenu()

const navItems = [
  { label: 'Dashboard', icon: 'pi pi-th-large', name: 'dashboard' },
]

function isActive(name) {
  return route.name === name
}
</script>
