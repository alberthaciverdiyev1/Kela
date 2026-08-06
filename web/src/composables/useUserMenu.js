import { ref, computed } from 'vue'
import { useAuthStore } from '../stores/auth'

// Navbar ve Sidebar için ortak kullanıcı menüsü.
// Ayarlar burada değil — rol bazlı nav menüsünde (teacher/student/parent).
export function useUserMenu() {
  const auth = useAuthStore()
  const menuRef = ref(null)

  const initials = computed(() => {
    const name = auth.fullName.trim()
    if (!name) return '?'
    const parts = name.split(' ')
    return (parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')
  })

  const userMenuItems = ref([
    {
      label: 'Çıkış Yap',
      icon: 'pi pi-sign-out',
      command: () => auth.logout(),
    },
  ])

  function toggleMenu(event) {
    menuRef.value?.toggle(event)
  }

  return { auth, initials, userMenuItems, menuRef, toggleMenu }
}
