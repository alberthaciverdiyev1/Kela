import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore, ROLES, homeRouteFor } from '../stores/auth'

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
        meta: { title: 'Dashboard', requiresRole: [ROLES.Teacher] },
      },
      {
        path: 'teacher/students',
        name: 'teacher.students',
        component: () => import('../views/teacher/Students.vue'),
        meta: { title: 'Öğrenciler', requiresRole: [ROLES.Teacher] },
      },
      {
        path: 'teacher/sections',
        name: 'teacher.sections',
        component: () => import('../views/common/ComingSoon.vue'),
        meta: { title: 'Sınıflar', requiresRole: [ROLES.Teacher] },
      },
      {
        path: 'teacher/settings',
        name: 'teacher.settings',
        component: () => import('../views/settings/Settings.vue'),
        meta: { title: 'Ayarlar', requiresRole: [ROLES.Teacher] },
      },

      // ── Student paneli ──
      {
        path: 'student/dashboard',
        name: 'student.dashboard',
        component: () => import('../views/student/StudentDashboard.vue'),
        meta: { title: 'Dashboard', requiresRole: [ROLES.Student] },
      },
      {
        path: 'student/courses',
        name: 'student.courses',
        component: () => import('../views/common/ComingSoon.vue'),
        meta: { title: 'Derslerim', requiresRole: [ROLES.Student] },
      },

      // ── Parent paneli ──
      {
        path: 'parent/dashboard',
        name: 'parent.dashboard',
        component: () => import('../views/parent/ParentDashboard.vue'),
        meta: { title: 'Dashboard', requiresRole: [ROLES.Parent] },
      },
      {
        path: 'parent/children',
        name: 'parent.children',
        component: () => import('../views/common/ComingSoon.vue'),
        meta: { title: 'Çocuklarım', requiresRole: [ROLES.Parent] },
      },
    ],
  },

  // Admin / bilinmeyen rol → ayrı yönetim paneli olduğu için buraya giremez
  {
    path: '/blocked',
    name: 'blocked',
    component: () => import('../views/NoAccess.vue'),
    meta: { title: 'Erişim Yok' },
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
  document.title = to.meta.title ? `${to.meta.title} · Kela` : 'Kela'
})

export default router
