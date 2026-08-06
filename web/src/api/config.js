import http from './http'

export const configApi = {
  getSiteConfig() {
    return http.get('/site-config')
  },

  updateSiteConfig(config) {
    return http.put('/site-config', config)
  },
}
