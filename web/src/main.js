import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import 'primeicons/primeicons.css'
import { setupPrimeVue } from './plugins/primevue'
import { useThemeStore } from './stores/theme'
import { useSettingsStore } from './stores/settings'
import './style.css'

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)

// Kaydedilmiş tercihleri uygula (ilk render'dan önce, flash olmadan)
useThemeStore(pinia).init()
useSettingsStore(pinia).init()

app.use(router)
setupPrimeVue(app)

app.mount('#app')
