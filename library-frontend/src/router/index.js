import { createRouter, createWebHistory } from 'vue-router'

const routes = [
  {
    path: '/',
    component: () => import('../views/HomeView.vue')
  },
  {
  path: '/home',
  component: () => import('../views/HomeView.vue')
},
  {
    path: '/login',
    component: () => import('../views/LoginView.vue')
  },
  {
    path: '/register',
    component: () => import('../views/RegisterView.vue')
  },
  {
    path: '/books/:id',
    component: () => import('../views/BookDetailView.vue')
  },
  {
    path: '/authors/:id',
    component: () => import('../views/AuthorDetailView.vue')
  },
  {
    path: '/categories/:id',
    component: () => import('../views/CategoryDetailView.vue')
  },
 {
  path: '/my-loans',
  component: () => import('../views/MyLoansView.vue'),
  meta: { requiresAuth: true }
},
{
  path: "/my-fines",
  component: () => import("../views/MyFinesView.vue"),
  meta: { requiresAuth: true }
},
{
  path: "/profile",
  component: () => import("../views/ProfileView.vue"),
  meta: { requiresAuth: true }
},
  {
  path: "/member",
  component: () => import("../views/MemberView.vue"),
  meta: { requiresAuth: true }
},
  {
  path: "/reports",
  component: () => import("../views/ReportsView.vue"),
  meta: { requiresAuth: true, role: 'Admin' }
},
  {
  path: '/admin',
  component: () => import('../views/AdminView.vue'),
  meta: { requiresAuth: true, role: 'Admin' }
},
{
  path: "/admin/books",
  component: () => import("../views/AdminBooksView.vue"),
  meta: { requiresAuth: true, role: 'Admin' }
},
{
  path: "/admin/loans",
  component: () => import("../views/AdminLoansView.vue"),
  meta: { requiresAuth: true, role: 'Admin' }
},
{
  path: "/admin/members",
  component: () => import("../views/AdminMembersView.vue"),
  meta: { requiresAuth: true, role: 'Admin' }
},
{
    path: '/admin/members/:id',
    component: () => import('../views/MemberDetailView.vue'),
    meta: { requiresAuth: true, role: 'Admin' }
  },
{
  path: "/admin/scan",
  name: "scan-book",
  component: () => import("@/views/ScanBookView.vue"),
  meta: { requiresAuth: true, role: 'Admin' }
},
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  const role = localStorage.getItem('role')

  const isLoggedIn = !!token
  const isAdmin = role === 'Admin'
  const isMember = role === 'Member' || role === 'Student'

  if (to.meta.requiresAuth && !isLoggedIn) {
    next('/login')
  } else if (to.meta.role === 'Admin' && !isAdmin) {
    next('/')
  } else if (to.meta.role === 'Member' && !isMember && !isAdmin) {
    next('/')
  } else {
    next()
  }
})

export default router