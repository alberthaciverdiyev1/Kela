<template>
  <div class="min-h-screen flex flex-col">
    <Toolbar class="border-0 border-bottom-1 border-surface-200 shadow-sm px-5 app-toolbar">
      <template #start>
        <div class="flex items-center gap-3">
          <Avatar icon="pi pi-book" shape="circle" class="bg-primary text-white" />
          <span class="font-bold text-xl text-primary">{{ config.siteName }}</span>
        </div>
      </template>

      <template #center>
        <IconField class="hidden md:flex w-72">
          <InputIcon class="pi pi-search" />
          <InputText placeholder="Ara..." class="w-full" />
        </IconField>
      </template>

      <template #end>
        <div class="flex items-center gap-1">
          <router-link
            v-for="item in navItems"
            :key="item.name"
            :to="{ name: item.name }"
            class="hidden lg:flex items-center gap-2 px-3 py-2 rounded-lg text-surface-600 transition-colors"
            :class="isActive(item.name)
              ? 'bg-primary-50 text-primary font-semibold'
              : 'hover:bg-surface-100 hover:text-primary'"
          >
            <i :class="['pi', item.icon]"></i>
            <span>{{ item.label }}</span>
          </router-link>
        </div>

        <div class="flex items-center gap-2">
          <Menu ref="menuRef" :model="userMenuItems" popup class="p-2" />
          <div class="flex items-center gap-2 cursor-pointer p-2 rounded-lg hover:bg-surface-100" @click="toggleMenu">
            <Avatar :label="initials" shape="circle" class="bg-primary text-white" />
            <span class="font-medium hidden sm:block">{{ auth.fullName }}</span>
            <i class="pi pi-chevron-down text-xs text-surface-500"></i>
          </div>
        </div>
      </template>
    </Toolbar>

    <main class="flex-1 p-6">
      <router-view />
    </main>

    <footer class="text-center text-sm text-surface-400 py-4">
      © {{ new Date().getFullYear() }} {{ config.siteName }}
    </footer>
  </div>
</template>

<script setup>
import { useRoute } from 'vue-router'
import { useUserMenu } from '../composables/useUserMenu'
import { useSiteConfigStore } from '../stores/siteConfig'

defineProps({
  navItems: { type: Array, default: () => [] },
})

const route = useRoute()
const config = useSiteConfigStore()
const { auth, initials, userMenuItems, menuRef, toggleMenu } = useUserMenu()

function isActive(name) {
  return route.name === name
}
</script>
