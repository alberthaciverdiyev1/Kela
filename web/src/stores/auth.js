import { defineStore } from 'pinia'
import { authApi } from '../api/auth'
import router from '../router'
import { useSiteConfigStore } from './siteConfig'
import { useI18nStore } from './i18n'

// Roller — Identity rol adları (backend Kela.Domain/Common/RoleNames.cs).
// Vue tarafı YALNIZCA öğrenci/öğretmen/veli içindir; Admin ayrı panelde yönetilir.
// Bu yüzden Admin burada tanımlı değildir — admin kullanıcısı girse bile
// teacher-only sayfalara erişemez, menüde admin öğesi görünmez.
export const ROLES = {
  Teacher: 'Teacher',
  Student: 'Student',
  Parent: 'Parent',
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,          // { userId, firstName, lastName, role } — login yanıtı
    loading: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.user,
    role: (state) => state.user?.role ?? null,
    fullName: (state) =>
      state.user ? `${state.user.firstName} ${state.user.lastName}` : '',
    roleName: (state) => {
      // Rol backend'den string gelir — aktif dile çevrilir.
      const i18n = useI18nStore()
      const role = state.user?.role
      if (!role) return i18n.t('common.unknownRole')
      const key = `role.${role}`
      const translated = i18n.t(key)
      // Bilinmeyen rol adları (Identity'e sonradan eklenenler) olduğu gibi gösterilir.
      return translated === key ? role : translated
    },
    isTeacher: (state) => state.user?.role === ROLES.Teacher,
  },

  actions: {
    async login(credentials) {
      this.loading = true
      try {
        this.user = await authApi.login(credentials)
        // Giriş sonrası sunucudaki site konfigürasyonunu uygula (tek GET)
        useSiteConfigStore().init()
        return { ok: true }
      } catch (error) {
        return { ok: false, message: error.message }
      } finally {
        this.loading = false
      }
    },

    async register(payload) {
      this.loading = true
      try {
        await authApi.register(payload)
        return { ok: true }
      } catch (error) {
        return { ok: false, message: error.message }
      } finally {
        this.loading = false
      }
    },

    async logout() {
      try {
        await authApi.logout()
      } catch {
        // 401/sunucu hatası olsa bile yerel oturumu temizle
      }
      this.user = null
      router.push({ name: 'login' })
    },
  },
})

// ─────────────────────────────────────────────────────────────
// ROL BAZLI PANEL YÖNLENDİRMESİ (A planı)
// Her rol kendi paneline düşer: /teacher/* , /student/* , /parent/*
// Admin bu uygulamada YOK — ayrı yönetim panelindedir, buraya girerse
// 'blocked' (Erişim Yok) sayfasına gider.
// ─────────────────────────────────────────────────────────────
export const ROLE_HOME = {
  [ROLES.Teacher]: 'teacher.dashboard',
  [ROLES.Student]: 'student.dashboard',
  [ROLES.Parent]: 'parent.dashboard',
}

export function homeRouteFor(role) {
  return ROLE_HOME[role] ?? 'blocked'
}
