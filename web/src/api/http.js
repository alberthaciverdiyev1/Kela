import axios from 'axios'
import router from '../router'
import { useI18nStore } from '../stores/i18n'


const http = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

http.interceptors.request.use((config) => {
  let lang
  try {
    lang = useI18nStore().locale
  } catch {
    lang = 'az'
  }
  config.params = { ...config.params, lang }
  config.headers = config.headers ?? {}
  config.headers['Accept-Language'] = lang
  return config
})

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
