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

window.__kelaAuthStore = useAuthStore(pinia)

useSiteConfigStore(pinia).init()

useI18nStore(pinia).setLocale(useI18nStore(pinia).locale)

app.use(router)
setupPrimeVue(app)

app.mount('#app')
