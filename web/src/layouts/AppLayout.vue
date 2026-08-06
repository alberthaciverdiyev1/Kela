<template>
  <!-- Rol bazlı panel menüsü + site config'e göre navbar/sidebar düzeni -->
  <NavbarLayout v-if="settings.isNavbar" :nav-items="navItems" />
  <SidebarLayout v-else :nav-items="navItems" />
</template>

<script setup>
import { useSiteConfigStore } from '../stores/siteConfig'
import { useNavItems } from '../composables/useNavItems'
import NavbarLayout from './NavbarLayout.vue'
import SidebarLayout from './SidebarLayout.vue'

const settings = useSiteConfigStore()
const { navItems } = useNavItems()
</script>
