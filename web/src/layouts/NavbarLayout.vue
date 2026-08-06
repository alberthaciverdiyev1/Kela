<template>
  <div class="min-h-screen flex flex-col">
    <Toolbar class="border-0 border-bottom-1 border-surface-200 shadow-sm px-5 app-toolbar">
      <template #start>
        <div class="flex items-center gap-3">
          <Avatar icon="pi pi-book" shape="circle" class="bg-primary text-white" />
          <span class="font-bold text-xl text-primary">{{ config.siteName }}</span>
        </div>
      </template>


      <template #end>
        <div class="flex items-center gap-2">
          <Menu ref="menuRef" :model="userMenuItems" popup class="p-2" />

          <Button
            text
            rounded
            icon="pi pi-th-large"
            label="Dashboard"
            @click="goDashboard"
            class="mr-2 hidden sm:inline-flex"
          />

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
import { useRouter } from 'vue-router'
import { useUserMenu } from '../composables/useUserMenu'
import { useSiteConfigStore } from '../stores/siteConfig'

const router = useRouter()
const config = useSiteConfigStore()
const { auth, initials, userMenuItems, menuRef, toggleMenu } = useUserMenu()

function goDashboard() {
  router.push({ name: 'dashboard' })
}
</script>
