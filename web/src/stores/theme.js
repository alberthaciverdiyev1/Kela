import { defineStore } from 'pinia'
import {
  COLOR_NAMES,
  DEFAULT_THEME,
  DEFAULT_BASES,
  deriveShades,
  applyTheme,
} from '../theme/tokens'

const STORAGE_KEY = 'kela.theme.bases'

// =========================================================
// Tema Store
// ---------------------------------------------------------
// • "Bütün sitenin renkleri" tek yerden yönetilir.
// • Öğretmen tema ayar sayfasında base rengi seçer → bu store
//   shade paletini üretir ve documentElement'e inline yazar.
// • KALICILIK:
//   Şu an localStorage (tarayıcı başına) ile saklanıyor.
//   Site genelinde herkesin görmesi için backend'e bir ayar
//   endpoint'i eklenince DEĞİŞTİRİLECEK YERLER:
//     - init()  → localStorage yerine API'den okuyacak
//     - save()  → localStorage yerine API'ye PUT yapacak
//   Yani dışarıdan hiçbir şey değişmez: sayfalar yalnızca
//   store'u kullanır.
// =========================================================
export const useThemeStore = defineStore('theme', {
  state: () => ({
    // Öğretmenin seçtiği base renkler (config UI bunu düzenler)
    bases: { ...DEFAULT_BASES },
    // Üretilmiş tam paletler (50-900) — single source of truth
    colors: JSON.parse(JSON.stringify(DEFAULT_THEME)),
    saved: true, // kaydedilmemiş değişiklik var mı?
  }),

  getters: {
    // Ayar sayfasının gösterdiği base renkler
    primaryBase: (s) => s.bases.primary,
    hasUnsavedChanges: (s) => !s.saved,
  },

  actions: {
    // Uygulama açılışında çağrılır (main.js)
    // TODO: backend endpoint hazır olunca localStorage yerine API'den yükle
    init() {
      try {
        const raw = localStorage.getItem(STORAGE_KEY)
        const saved = raw ? JSON.parse(raw) : {}
        if (saved && typeof saved === 'object') {
          for (const name of COLOR_NAMES) {
            const hex = saved[name]
            if (hex && /^#[0-9a-fA-F]{6}$/.test(hex)) {
              this.bases[name] = hex
              this.colors[name] = deriveShades(hex)
            }
          }
        }
      } catch {
        /* bozuk veri → varsayılan tema */
      }
      applyTheme(this.colors)
      this.saved = true
    },

    // Base rengi değişti → paleti üret, canlı uygula (kaydetmez)
    setBase(name, hex) {
      if (!COLOR_NAMES.includes(name) || !/^#[0-9a-fA-F]{6}$/.test(hex)) return
      this.bases[name] = hex
      this.colors[name] = deriveShades(hex)
      applyTheme(this.colors)
      this.saved = false
    },

    // Varsayılan renkleri geri yükle
    reset() {
      this.bases = { ...DEFAULT_BASES }
      this.colors = JSON.parse(JSON.stringify(DEFAULT_THEME))
      localStorage.removeItem(STORAGE_KEY)
      applyTheme(this.colors)
      this.saved = true
    },

    // Değişiklikleri kalıcı yap
    // TODO: backend endpoint hazır olunca localStorage yerine API'ye kaydet
    save() {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.bases))
      this.saved = true
    },
  },
})
