import { defineStore } from 'pinia'

const STORAGE_KEY = 'kela.settings.navMode'

// Navigasyon modları (tek kaynak noktası)
export const NAV_MODES = {
  navbar: 'navbar',
  sidebar: 'sidebar',
}

export const NAV_MODE_LABELS = {
  navbar: 'Üst Menü (Navbar)',
  sidebar: 'Yan Menü (Sidebar)',
}

// =========================================================
// Kullanıcı tercihleri store'u
// ---------------------------------------------------------
// Kişisel tercihler (tarayıcı başına). Ortak ayarlar değil:
// her kullanıcı kendi navigasyon düzenini seçer.
// =========================================================
export const useSettingsStore = defineStore('settings', {
  state: () => ({
    navMode: NAV_MODES.navbar,
  }),

  getters: {
    isSidebar: (s) => s.navMode === NAV_MODES.sidebar,
    isNavbar: (s) => s.navMode === NAV_MODES.navbar,
  },

  actions: {
    // Uygulama açılışında çağrılır (main.js)
    init() {
      try {
        const saved = localStorage.getItem(STORAGE_KEY)
        if (saved === NAV_MODES.sidebar) this.navMode = NAV_MODES.sidebar
        else this.navMode = NAV_MODES.navbar
      } catch {
        this.navMode = NAV_MODES.navbar
      }
    },

    setNavMode(mode) {
      if (!Object.values(NAV_MODES).includes(mode)) return
      this.navMode = mode
      try {
        localStorage.setItem(STORAGE_KEY, mode)
      } catch {
        /* depolama yoksa sadece oturum içinde geçerli */
      }
    },
  },
})
