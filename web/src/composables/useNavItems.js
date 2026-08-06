import { computed } from 'vue'
import { useAuthStore, ROLES } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'

export function useNavItems() {
  const auth = useAuthStore()
  const i18n = useI18nStore()

  const navItems = computed(() => {
    const t = i18n.t
    switch (auth.role) {
      case ROLES.Teacher:
        return [
          { label: t('nav.dashboard'), icon: 'pi pi-th-large', name: 'teacher.dashboard' },
          { label: t('nav.students'), icon: 'pi pi-user', name: 'teacher.students' },
          { label: t('nav.classes'), icon: 'pi pi-users', name: 'teacher.sections' },
          { label: t('nav.settings'), icon: 'pi pi-cog', name: 'teacher.settings' },
        ]
      case ROLES.Student:
        return [
          { label: t('nav.dashboard'), icon: 'pi pi-th-large', name: 'student.dashboard' },
          { label: t('nav.myCourses'), icon: 'pi pi-book', name: 'student.courses' },
        ]
      case ROLES.Parent:
        return [
          { label: t('nav.dashboard'), icon: 'pi pi-th-large', name: 'parent.dashboard' },
          { label: t('nav.myChildren'), icon: 'pi pi-users', name: 'parent.children' },
        ]
      default:
        return []
    }
  })

  return { navItems }
}
