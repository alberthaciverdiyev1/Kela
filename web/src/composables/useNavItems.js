import { computed } from 'vue'
import { useAuthStore, ROLES } from '../stores/auth'

// Rol bazlı panel menüsü. Her rol kendi öğelerini görür;
// başka rolün öğesi asla gösterilmez. Yeni panel öğesi eklemek için
// role ait diziye giriş eklemek + router'a karşılık gelen rota açmak yeterli.
export function useNavItems() {
  const auth = useAuthStore()

  const navItems = computed(() => {
    switch (auth.role) {
      case ROLES.Teacher:
        return [
          { label: 'Dashboard', icon: 'pi pi-th-large', name: 'teacher.dashboard' },
          { label: 'Sınıflar', icon: 'pi pi-users', name: 'teacher.sections' },
          { label: 'Ayarlar', icon: 'pi pi-cog', name: 'teacher.settings' },
        ]
      case ROLES.Student:
        return [
          { label: 'Dashboard', icon: 'pi pi-th-large', name: 'student.dashboard' },
          { label: 'Derslerim', icon: 'pi pi-book', name: 'student.courses' },
        ]
      case ROLES.Parent:
        return [
          { label: 'Dashboard', icon: 'pi pi-th-large', name: 'parent.dashboard' },
          { label: 'Çocuklarım', icon: 'pi pi-users', name: 'parent.children' },
        ]
      default:
        return []
    }
  })

  return { navItems }
}
