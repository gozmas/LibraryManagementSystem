import { defineStore } from 'pinia'
import api from '../api/axios'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: localStorage.getItem('token') || null,
    user: JSON.parse(localStorage.getItem('user') || 'null')
  }),

  getters: {
    isLoggedIn: (state) => !!state.token,
    isAdmin: (state) => state.user?.role === 'Admin',
    isMember: (state) => state.user?.role === 'Member'
  },

  actions: {
    async login(email, password) {
      const response = await api.post('/Auth/login', { email, password })
      const { token, username, email: userEmail, role } = response.data.data
      this.token = token
      this.user = { username, email: userEmail, role }
      localStorage.setItem('token', token)
      localStorage.setItem('user', JSON.stringify(this.user))
      return role
    },

    async register(data) {
      const response = await api.post('/Auth/register', data)
      return response.data
    },

    logout() {
      this.token = null
      this.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    }
  }
})