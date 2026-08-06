import { defineStore } from 'pinia'
import { messages } from '../i18n/messages'

// ─────────────────────────────────────────────────────────────
// DİL / YERELSELLEŞTİRME STORE'U
// 4 dil: az (varsayılan) / en / ru / tr. Seçim localStorage'da
// "kela.lang" anahtarıyla saklanır; http.js her API isteğine
// bu dili `lang` parametresi + Accept-Language header olarak ekler.
// ─────────────────────────────────────────────────────────────

export const LANGS = ['az', 'en', 'ru', 'tr']
export const DEFAULT_LANG = 'az'

export const LANG_OPTIONS = [
  { code: 'az', label: 'Azərbaycanca' },
  { code: 'en', label: 'English' },
  { code: 'ru', label: 'Русский' },
  { code: 'tr', label: 'Türkçe' },
]

export const LANG_NAMES = {
  az: 'Azərbaycanca',
  en: 'English',
  ru: 'Русский',
  tr: 'Türkçe',
}

function loadLocale() {
  try {
    const saved = localStorage.getItem('kela.lang')
    return LANGS.includes(saved) ? saved : DEFAULT_LANG
  } catch {
    return DEFAULT_LANG
  }
}

export const useI18nStore = defineStore('i18n', {
  state: () => ({
    locale: loadLocale(),
  }),

  getters: {
    // Reaktif çeviri: t('key') veya t('key', { name: 'Ali' })
    // Eksik anahtar → en'e, o da yoksa anahtarın kendisine düşer.
    t: (state) => (key, params) => {
      const dict = messages[state.locale] ?? messages[DEFAULT_LANG]
      let text = dict[key] ?? messages.en[key] ?? key
      if (params) {
        for (const [k, v] of Object.entries(params)) {
          text = text.replaceAll(`{${k}}`, String(v))
        }
      }
      return text
    },

    langLabel: (state) => LANG_NAMES[state.locale] ?? LANG_NAMES[DEFAULT_LANG],
  },

  actions: {
    setLocale(code) {
      if (!LANGS.includes(code)) code = DEFAULT_LANG
      this.locale = code
      try {
        localStorage.setItem('kela.lang', code)
      } catch {
        /* saklama yoksa sadece oturum için geçerli */
      }
      if (typeof document !== 'undefined') {
        document.documentElement.lang = code
      }
    },
  },
})

// Bileşenlerde kullanım kolaylığı: const i18n = useI18n()
export function useI18n() {
  return useI18nStore()
}
