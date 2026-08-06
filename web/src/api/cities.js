import http from './http'

// Şehirler — çevirili adlarla (lang parametresi). Dropdown/yönetim için.
export const citiesApi = {
  getPage(page = 1, pageSize = 100, lang = 'tr') {
    return http.get('/cities', { params: { page, pageSize, lang } })
  },
}
