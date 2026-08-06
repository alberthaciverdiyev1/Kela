import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore, ROLES } from '../stores/auth'

const routes = [
  {
    path: '/',
    component: () => import('../layouts/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: '/dashboard' },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('../views/dashboard/Dashboard.vue'),
        meta: { title: 'Dashboard' },
      },
      {
        path: 'settings/theme',
        name: 'settings-theme',
        component: () => import('../views/settings/Theme.vue'),
        meta: { title: 'Site Tasarımı', requiresRole: [ROLES.Admin, ROLES.Teacher] },
      },
      {
        path: 'settings/layout',
        name: 'settings-layout',
        component: () => import('../views/settings/Layout.vue'),
        meta: { title: 'Arayüz Düzeni' },
      },
    ],
  },
  {
    path: '/auth',
    component: () => import('../layouts/AuthLayout.vue'),
    meta: { guestOnly: true },
    children: [
      {
        path: 'login',
        name: 'login',
        component: () => import('../views/auth/Login.vue'),
        meta: { title: 'Giriş Yap' },
      },
      {
        path: 'register',
        name: 'register',
        component: () => import('../views/auth/Register.vue'),
        meta: { title: 'Kayıt Ol' },
      },
    ],
  },
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// Global guard: auth gerektiren rotaları kontrol et
router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.guestOnly && auth.isAuthenticated) {
    return { name: 'dashboard' }
  }

  // Rol korumalı rotalar: yetersiz yetki → dashboard'a yönlendir
  if (to.meta.requiresRole && !to.meta.requiresRole.includes(auth.role)) {
    return { name: 'dashboard' }
  }

  return true
})

router.afterEach((to) => {
  document.title = to.meta.title ? `${to.meta.title} · Kela` : 'Kela'
})

export default router
