import { ref, computed } from 'vue'
import { useAuthStore } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'

export function useUserMenu() {
  const auth = useAuthStore()
  const i18n = useI18nStore()
  const menuRef = ref(null)

  const initials = computed(() => {
    const name = auth.fullName.trim()
    if (!name) return '?'
    const parts = name.split(' ')
    return (parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')
  })

  const userMenuItems = computed(() => [
    {
      label: i18n.t('nav.logout'),
      icon: 'pi pi-sign-out',
      command: () => auth.logout(),
    },
  ])

  function toggleMenu(event) {
    menuRef.value?.toggle(event)
  }

  return { auth, initials, userMenuItems, menuRef, toggleMenu }
}
