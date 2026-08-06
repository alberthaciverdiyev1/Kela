import http from './http'

// ─────────────────────────────────────────────────────────────
// SİTE KONFİGÜRASYONU — TEK entity, TEK request, TEK response
// -------------------------------------------------------------
//   GET /api/site-config  → tüm site ayarları (giriş yapan herkes)
//   PUT /api/site-config  → tüm site ayarlarını güncelle (Admin/Teacher)
//
// Yeni bir site ayarı eklendiğinde backend'e buraya da eklemek gerekmez:
// response zaten tüm alanları getirir. İhtiyaç olursa tek metot kalır.
// ─────────────────────────────────────────────────────────────
export const configApi = {
  // Tüm site konfigürasyonunu oku
  getSiteConfig() {
    return http.get('/site-config')
  },

  // Tüm site konfigürasyonunu tek istekte güncelle
  updateSiteConfig(config) {
    return http.put('/site-config', config)
  },
}
