<template>
  <!-- Kullanıcı tercihine göre navigasyon düzeni (settings store) -->
  <NavbarLayout v-if="settings.isNavbar" />
  <SidebarLayout v-else />
</template>

<script setup>
import { useSettingsStore } from '../stores/settings'
import NavbarLayout from './NavbarLayout.vue'
import SidebarLayout from './SidebarLayout.vue'

const settings = useSettingsStore()
</script>
