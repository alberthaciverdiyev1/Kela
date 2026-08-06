import http from './http'

export const studentsApi = {
  getPage(page = 1, pageSize = 10, lang = 'tr') {
    return http.get('/students', { params: { page, pageSize, lang } })
  },

  create(payload) {
    return http.post('/students', payload)
  },

  remove(id) {
    return http.delete(`/students/${id}`)
  },
}
