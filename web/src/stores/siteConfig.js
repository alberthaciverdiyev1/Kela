import { defineStore } from 'pinia'
import { configApi } from '../api/config'
import {
  COLOR_NAMES,
  DEFAULT_THEME,
  DEFAULT_BASES,
  deriveShades,
  applyTheme,
} from '../theme/tokens'

const CACHE_KEY = 'kela.siteConfig'

const NAV_MODES = { navbar: 'navbar', sidebar: 'sidebar' }

// =========================================================
// TEK SİTE KONFİGÜRASYON STORE'U
// ---------------------------------------------------------
// Backend'deki BaseSiteConfiguration entity'sinin birebir karşılığı.
// Tüm site ayarları (renkler + arayüz düzeni + site adı) tek store'da,
// tek init() ile yüklenir, tek save() ile kaydedilir.
// Gelecekte yeni bir alan eklendiğinde: backend'e property + buraya state
// eklemek yeterli — tüm kullanıcılar aynı GET ile otomatik görür.
// =========================================================
export const useSiteConfigStore = defineStore('siteConfig', {
  state: () => ({
    siteName: 'Kela LMS',
    bases: { ...DEFAULT_BASES },
    colors: JSON.parse(JSON.stringify(DEFAULT_THEME)),
    navMode: NAV_MODES.navbar,
    saved: true,
    loading: false,
    error: '',
  }),

  getters: {
    isSidebar: (s) => s.navMode === NAV_MODES.sidebar,
    isNavbar: (s) => s.navMode === NAV_MODES.navbar,
    hasUnsavedChanges: (s) => !s.saved,
  },

  actions: {
    // Uygulama açılışında / giriş sonrası: önce önbellek (flash yok),
    // sonra sunucudan güncel tek response.
    async init() {
      this.loadFromCache()
      await this.refresh()
    },

    // Sunucudan tüm site konfigürasyonunu çek ve uygula
    async refresh() {
      try {
        const config = await configApi.getSiteConfig()
        this.applyConfig(config)
        this.saved = true
      } catch {
        // oturum yok / sunucu yok → önbellek/varsayılan ile devam
      }
    },

    // Tek response'u state'e uygula
    applyConfig(config) {
      if (!config) return
      if (config.siteName) this.siteName = config.siteName
      this.applyBases({
        primary: config.primaryColor,
        secondary: config.secondaryColor,
        success: config.successColor,
        warning: config.warningColor,
        error: config.errorColor,
        info: config.infoColor,
      })
      if (config.navMode === NAV_MODES.navbar || config.navMode === NAV_MODES.sidebar) {
        this.navMode = config.navMode
      }
    },

    // Yerel önbellekten hızlı uygula (ilk boyama)
    loadFromCache() {
      try {
        const raw = localStorage.getItem(CACHE_KEY)
        const cached = raw ? JSON.parse(raw) : {}
        if (cached.siteName) this.siteName = cached.siteName
        if (cached.bases) this.applyBases(cached.bases)
        if (cached.navMode === NAV_MODES.navbar || cached.navMode === NAV_MODES.sidebar) {
          this.navMode = cached.navMode
        }
      } catch {
        /* bozuk önbellek → varsayılan */
      }
      applyTheme(this.colors)
    },

    applyBases(bases) {
      for (const name of COLOR_NAMES) {
        const hex = bases?.[name]
        if (hex && /^#[0-9a-fA-F]{6}$/.test(hex)) {
          this.bases[name] = hex
          this.colors[name] = deriveShades(hex)
        }
      }
      applyTheme(this.colors)
    },

    // ── Yerel düzenlemeler (canlı önizleme, kaydetmez) ──

    setSiteName(value) {
      this.siteName = value
      this.saved = false
    },

    setBase(name, hex) {
      if (!COLOR_NAMES.includes(name) || !/^#[0-9a-fA-F]{6}$/.test(hex)) return
      this.bases[name] = hex
      this.colors[name] = deriveShades(hex)
      applyTheme(this.colors)
      this.saved = false
    },

    setNavMode(mode) {
      if (![NAV_MODES.navbar, NAV_MODES.sidebar].includes(mode)) return
      this.navMode = mode
      this.saved = false
    },

    reset() {
      this.siteName = 'Kela LMS'
      this.bases = { ...DEFAULT_BASES }
      this.colors = JSON.parse(JSON.stringify(DEFAULT_THEME))
      this.navMode = NAV_MODES.navbar
      localStorage.removeItem(CACHE_KEY)
      applyTheme(this.colors)
      this.saved = false
    },

    // ── Tek istekte tüm site konfigürasyonunu kaydet ──
    async save() {
      this.loading = true
      this.error = ''
      try {
        const payload = {
          siteName: this.siteName,
          primaryColor: this.bases.primary,
          secondaryColor: this.bases.secondary,
          successColor: this.bases.success,
          warningColor: this.bases.warning,
          errorColor: this.bases.error,
          infoColor: this.bases.info,
          navMode: this.navMode,
        }
        await configApi.updateSiteConfig(payload)
        localStorage.setItem(CACHE_KEY, JSON.stringify({
          siteName: this.siteName,
          bases: this.bases,
          navMode: this.navMode,
        }))
        this.saved = true
        return { ok: true }
      } catch (error) {
        this.error = error.message
        return { ok: false, message: error.message }
      } finally {
        this.loading = false
      }
    },
  },
})
