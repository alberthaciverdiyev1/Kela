import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import basicSsl from '@vitejs/plugin-basic-ssl'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue(), tailwindcss(), basicSsl()],
  server: {
    https: true,                 // Secure cookie için HTTPS dev server
    port: 5173,
    proxy: {
      '/api': {
        target: 'https://localhost:7047',  // Kela.Api
        changeOrigin: true,
        secure: false,                    // yerel dev sertifikasına güven
      },
    },
  },
})
