import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'
import 'primeicons/primeicons.css'
import { setupPrimeVue } from './plugins/primevue'
import { useThemeStore } from './stores/theme'
import './style.css'

const app = createApp(App)

const pinia = createPinia()
app.use(pinia)

// Kaydedilmiş temayı uygula (ilk render'dan önce, flash olmadan)
useThemeStore(pinia).init()

app.use(router)
setupPrimeVue(app)

app.mount('#app')
