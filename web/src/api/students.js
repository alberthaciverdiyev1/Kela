import http from './http'

// ─────────────────────────────────────────────────────────────
// ÖĞRENCİ YÖNETİMİ (Teacher paneli)
// -------------------------------------------------------------
//   GET    /api/students?page&pageSize&lang=tr → sayfalı liste
//   POST   /api/students → sistem mail+şifre üretir, yanıtta döner
//   DELETE /api/students/{id} → soft delete
//
// POST yanıtı: { id, userId, email, password, createdAt }
//   — mail & şifre yalnızca oluşturma anında döner; öğrenciye iletilir.
// ─────────────────────────────────────────────────────────────
export const studentsApi = {
  getPage(page = 1, pageSize = 10, lang = 'tr') {
    return http.get('/students', { params: { page, pageSize, lang } })
  },

  create(payload) {
    // { firstName, lastName, phoneNumber?, birthDate?, cityId? } — email/password SİSTEM üretir
    return http.post('/students', payload)
  },

  remove(id) {
    return http.delete(`/students/${id}`)
  },
}
