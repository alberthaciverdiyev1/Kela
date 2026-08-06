import http from './http'

// Şehirler — çevirili adlarla (lang parametresi). http.js interceptor'ı aktif dili otomatik ekler.
export const citiesApi = {
  getPage(page = 1, pageSize = 100, lang = 'az') {
    return http.get('/cities', { params: { page, pageSize, lang } })
  },
}
