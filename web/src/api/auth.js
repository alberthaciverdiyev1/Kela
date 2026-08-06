import http from './http'

export const authApi = {
  login(credentials) {
    return http.post('/auth/login', credentials)
  },

  register(payload) {
    return http.post('/auth/register', payload)
  },

  logout() {
    return http.post('/auth/logout')
  },
}
