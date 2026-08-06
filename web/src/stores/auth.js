import { defineStore } from 'pinia'
import { authApi } from '../api/auth'
import router from '../router'
import { useSiteConfigStore } from './siteConfig'
import { useI18nStore } from './i18n'

export const ROLES = {
  Teacher: 'Teacher',
  Student: 'Student',
  Parent: 'Parent',
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    loading: false,
  }),

  getters: {
    isAuthenticated: (state) => !!state.user,
    role: (state) => state.user?.role ?? null,
    fullName: (state) =>
      state.user ? `${state.user.firstName} ${state.user.lastName}` : '',
    roleName: (state) => {
      const i18n = useI18nStore()
      const role = state.user?.role
      if (!role) return i18n.t('common.unknownRole')
      const key = `role.${role}`
      const translated = i18n.t(key)
      return translated === key ? role : translated
    },
    isTeacher: (state) => state.user?.role === ROLES.Teacher,
  },

  actions: {
    async login(credentials) {
      this.loading = true
      try {
        this.user = await authApi.login(credentials)
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
      }
      this.user = null
      router.push({ name: 'login' })
    },
  },
})

export const ROLE_HOME = {
  [ROLES.Teacher]: 'teacher.dashboard',
  [ROLES.Student]: 'student.dashboard',
  [ROLES.Parent]: 'parent.dashboard',
}

export function homeRouteFor(role) {
  return ROLE_HOME[role] ?? 'blocked'
}
