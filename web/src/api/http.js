import axios from 'axios'
import router from '../router'

// Backend zarfı: { statusCode, success, message, data, errors }
// HTTP ile aynı origin'den (Vite proxy /api) gidilir → cookie otomatik gönderilir.

const http = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

// Yanıt: envelope'i açar, "data"yı doğrudan döndürür.
http.interceptors.response.use(
  (response) => {
    const env = response.data
    if (env && typeof env === 'object' && 'success' in env) {
      if (!env.success) {
        return Promise.reject(new Error(env.message || 'İstek başarısız oldu.'))
      }
      return env.data ?? env
    }
    return response.data
  },
  (error) => {
    // API 401 döndürürse (oturum yok/geçersiz) → login'e yönlendir
    if (error.response && error.response.status === 401) {
      const auth = window.__kelaAuthStore
      if (auth) auth.logout()
      router.push({ name: 'login' })
    }
    const message =
      error.response?.data?.message ||
      error.response?.data?.errors?.[0] ||
      error.message ||
      'Bir hata oluştu.'
    return Promise.reject(new Error(message))
  },
)

export default http
