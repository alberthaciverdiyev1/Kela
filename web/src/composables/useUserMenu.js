import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

// Hem NavbarLayout hem SidebarLayout için ortak kullanıcı menüsü
export function useUserMenu() {
  const auth = useAuthStore()
  const router = useRouter()
  const menuRef = ref(null)

  const initials = computed(() => {
    const name = auth.fullName.trim()
    if (!name) return '?'
    const parts = name.split(' ')
    return (parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')
  })

  const userMenuItems = ref([])

  // Profil + (öğretmen/admin için) Site Tasarımı + Arayüz Düzeni + Çıkış
  userMenuItems.value = [
    {
      label: 'Profil',
      icon: 'pi pi-user',
      command: () => router.push({ name: 'profile' }),
    },
    {
      label: 'Arayüz Düzeni',
      icon: 'pi pi-bars',
      command: () => router.push({ name: 'settings-layout' }),
    },
  ]

  if (auth.isTeacher || auth.isAdmin) {
    userMenuItems.value.push({
      label: 'Site Tasarımı',
      icon: 'pi pi-palette',
      command: () => router.push({ name: 'settings-theme' }),
    })
  }

  userMenuItems.value.push(
    { separator: true },
    {
      label: 'Çıkış Yap',
      icon: 'pi pi-sign-out',
      command: () => auth.logout(),
    },
  )

  function toggleMenu(event) {
    menuRef.value?.toggle(event)
  }

  return { auth, initials, userMenuItems, menuRef, toggleMenu }
}
