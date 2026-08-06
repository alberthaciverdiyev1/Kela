import { defineStore } from 'pinia'
import { authApi } from '../api/auth'
import router from '../router'
import { useSiteConfigStore } from './siteConfig'

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
      // Rol artık backend'den string gelir — önce ROLES'ta mı kontrol et,
      // bilinmeyen rol adlarını da göster (Identity'e sonradan eklenen roller).
      return ROLES[state.user?.role] ?? state.user?.role ?? 'Bilinmiyor'
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
