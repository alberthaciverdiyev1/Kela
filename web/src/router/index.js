import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore, ROLES, homeRouteFor } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'

// ─────────────────────────────────────────────────────────────
// A PLANI — ROL BAZLI PANEL AĞAÇLARI
// Tek uygulama, üç ayrı panel. Her rol kendi ağacına sahiptir:
//   /teacher/*  → TeacherLayout (Ayarlar, Sınıflar...)
//   /student/*  → StudentLayout (Derslerim...)
//   /parent/*   → ParentLayout  (Çocuklarım...)
// Başka bir rolün paneline URL'den bile girilemez (guard).
// Admin burada değildir → 'blocked' sayfasına gider.
// ─────────────────────────────────────────────────────────────
const routes = [
  {
    path: '/',
    component: () => import('../layouts/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      // Kök → rolün kendi paneline yönlendir
      { path: '', redirect: () => ({ name: homeRouteFor(useAuthStore().role) }) },

      // ── Teacher paneli ──
      {
        path: 'teacher/dashboard',
        name: 'teacher.dashboard',
        component: () => import('../views/teacher/TeacherDashboard.vue'),
        meta: { title: 'nav.dashboard', requiresRole: [ROLES.Teacher] },
      },
      {
        path: 'teacher/students',
        name: 'teacher.students',
        component: () => import('../views/teacher/Students.vue'),
        meta: { title: 'nav.students', requiresRole: [ROLES.Teacher] },
      },
      {
        path: 'teacher/sections',
        name: 'teacher.sections',
        component: () => import('../views/common/ComingSoon.vue'),
        meta: { title: 'nav.classes', requiresRole: [ROLES.Teacher] },
      },
      {
        path: 'teacher/settings',
        name: 'teacher.settings',
        component: () => import('../views/settings/Settings.vue'),
        meta: { title: 'nav.settings', requiresRole: [ROLES.Teacher] },
      },

      // ── Student paneli ──
      {
        path: 'student/dashboard',
        name: 'student.dashboard',
        component: () => import('../views/student/StudentDashboard.vue'),
        meta: { title: 'nav.dashboard', requiresRole: [ROLES.Student] },
      },
      {
        path: 'student/courses',
        name: 'student.courses',
        component: () => import('../views/common/ComingSoon.vue'),
        meta: { title: 'nav.myCourses', requiresRole: [ROLES.Student] },
      },

      // ── Parent paneli ──
      {
        path: 'parent/dashboard',
        name: 'parent.dashboard',
        component: () => import('../views/parent/ParentDashboard.vue'),
        meta: { title: 'nav.dashboard', requiresRole: [ROLES.Parent] },
      },
      {
        path: 'parent/children',
        name: 'parent.children',
        component: () => import('../views/common/ComingSoon.vue'),
        meta: { title: 'nav.myChildren', requiresRole: [ROLES.Parent] },
      },
    ],
  },

  // Admin / bilinmeyen rol → ayrı yönetim paneli olduğu için buraya giremez
  {
    path: '/blocked',
    name: 'blocked',
    component: () => import('../views/NoAccess.vue'),
    meta: { title: 'noAccess.title' },
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
        meta: { title: 'auth.login' },
      },
      {
        path: 'register',
        name: 'register',
        component: () => import('../views/auth/Register.vue'),
        meta: { title: 'auth.register' },
      },
    ],
  },

  // Bilinmeyen yol → rolün kendi paneline
  {
    path: '/:pathMatch(.*)*',
    redirect: () => ({ name: homeRouteFor(useAuthStore().role) }),
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// Global guard:
//  - auth gereken rota + oturum yok → login
//  - guestOnly + oturum var → kendi paneline
//  - rol korumalı rota + yetki yok → kendi paneline (admin → blocked)
router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  if (to.meta.guestOnly && auth.isAuthenticated) {
    return { name: homeRouteFor(auth.role) }
  }

  if (to.meta.requiresRole && !to.meta.requiresRole.includes(auth.role)) {
    return { name: homeRouteFor(auth.role) }
  }

  return true
})

router.afterEach((to) => {
  const i18n = useI18nStore()
  const title = to.meta.title ? i18n.t(to.meta.title) : 'Kela'
  document.title = `${title} · Kela`
})

export default router
