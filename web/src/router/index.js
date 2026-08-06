import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore, ROLES, homeRouteFor } from '../stores/auth'
import { useI18nStore } from '../stores/i18n'

const routes = [
  {
    path: '/',
    component: () => import('../layouts/AppLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: () => ({ name: homeRouteFor(useAuthStore().role) }) },

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

  {
    path: '/:pathMatch(.*)*',
    redirect: () => ({ name: homeRouteFor(useAuthStore().role) }),
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

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
