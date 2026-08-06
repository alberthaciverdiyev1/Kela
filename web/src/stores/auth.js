import { defineStore } from 'pinia'
import { authApi } from '../api/auth'
import router from '../router'

// Roller (backend Kela.Domain/Enums/Role.cs ile eşleşir)
export const ROLES = {
  Admin: 1,
  Teacher: 2,
  Student: 3,
  Parent: 4,
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
      const names = {
        [ROLES.Admin]: 'Admin',
        [ROLES.Teacher]: 'Teacher',
        [ROLES.Student]: 'Student',
        [ROLES.Parent]: 'Parent',
      }
      return names[state.user?.role] ?? 'Bilinmiyor'
    },
    isTeacher: (state) => state.user?.role === ROLES.Teacher,
    isAdmin: (state) => state.user?.role === ROLES.Admin,
  },

  actions: {
    async login(credentials) {
      this.loading = true
      try {
        this.user = await authApi.login(credentials)
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
