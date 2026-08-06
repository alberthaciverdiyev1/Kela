import http from './http'

export const citiesApi = {
  getPage(page = 1, pageSize = 100, lang = 'az') {
    return http.get('/cities', { params: { page, pageSize, lang } })
  },
}
