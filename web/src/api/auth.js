import http from './http'

export const authApi = {
  // POST /api/auth/login → Set-Cookie (Kela.Auth) + { userId, firstName, lastName, role }
  login(credentials) {
    return http.post('/auth/login', credentials)
  },

  // POST /api/auth/register → { userId, firstName, lastName, email }  (rol: Teacher)
  register(payload) {
    return http.post('/auth/register', payload)
  },

  // POST /api/auth/logout
  logout() {
    return http.post('/auth/logout')
  },
}
