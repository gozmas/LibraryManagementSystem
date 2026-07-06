import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

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
  component: () => import('../views/MyLoansView.vue')
},
{
  path: "/my-fines",
  component: () => import("../views/MyFinesView.vue")
},
{
  path: "/profile",
  component: () => import("../views/ProfileView.vue")
},
  {
  path: "/member",
  component: () => import("../views/MemberView.vue")
},
  {
  path: "/reports",
  component: () => import("../views/ReportsView.vue")
},
  {
  path: '/admin',
  component: () => import('../views/AdminView.vue')
},
{
  path: "/admin/books",
  component: () => import("../views/AdminBooksView.vue")
},
{
  path: "/admin/loans",
  component: () => import("../views/AdminLoansView.vue")
},
{
  path: "/admin/members",
  component: () => import("../views/AdminMembersView.vue"),
},
{
    path: '/admin/members/:id',
    component: () => import('../views/MemberDetailView.vue')
  },
{
  path: "/admin/scan",
  name: "scan-book",
  component: () => import("@/views/ScanBookView.vue"),
},
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isLoggedIn) {
    next('/login')
  } else if (to.meta.role === 'Admin' && !auth.isAdmin) {
    next('/')
  } else if (to.meta.role === 'Member' && !auth.isMember && !auth.isAdmin) {
    next('/')
  } else {
    next()
  }
})

export default router