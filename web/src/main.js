import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import 'primeicons/primeicons.css'
import { setupPrimeVue } from './plugins/primevue'
import { useAuthStore } from './stores/auth'
import { useSiteConfigStore } from './stores/siteConfig'
import './style.css'

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)

// http.js 401 handler'ı çıkışı tetikleyebilsin diye auth store'u bağla
window.__kelaAuthStore = useAuthStore(pinia)

// Site konfigürasyonunu uygula (ilk render'dan önce, flash olmadan).
// init() önce yerel önbelleği senkron uygular, sonra backend'den tek GET ile çeker.
useSiteConfigStore(pinia).init()

app.use(router)
setupPrimeVue(app)

app.mount('#app')
