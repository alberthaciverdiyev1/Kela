<template>
  <!-- Site konfigürasyonundaki arayüz düzenine göre navigasyon (navbar/sidebar) -->
  <NavbarLayout v-if="settings.isNavbar" />
  <SidebarLayout v-else />
</template>

<script setup>
import { useSiteConfigStore } from '../stores/siteConfig'
import NavbarLayout from './NavbarLayout.vue'
import SidebarLayout from './SidebarLayout.vue'

const settings = useSiteConfigStore()
</script>
