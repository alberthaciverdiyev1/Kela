import { createApp } from 'vue'
import { createPinia, setActivePinia } from 'pinia'
import App from './App.vue'
import router from './router'
import 'primeicons/primeicons.css'
import { setupPrimeVue } from './plugins/primevue'
import { useAuthStore } from './stores/auth'
import { useSiteConfigStore } from './stores/siteConfig'
import { useI18nStore } from './stores/i18n'
import './style.css'

const app = createApp(App)

const pinia = createPinia()
setActivePinia(pinia) // http.js interceptor'ı store'a pinia olmadan erişebilsin
app.use(pinia)

// http.js 401 handler'ı çıkışı tetikleyebilsin diye auth store'u bağla
window.__kelaAuthStore = useAuthStore(pinia)

// Site konfigürasyonunu uygula (ilk render'dan önce, flash olmadan).
// init() önce yerel önbelleği senkron uygular, sonra backend'den tek GET ile çeker.
useSiteConfigStore(pinia).init()

// Varsayılan dil az; <html lang="az"> set et
useI18nStore(pinia).setLocale(useI18nStore(pinia).locale)

app.use(router)
setupPrimeVue(app)

app.mount('#app')
