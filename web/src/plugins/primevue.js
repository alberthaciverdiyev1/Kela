import PrimeVue from 'primevue/config'
import Aura from '@primeuix/themes/aura'

import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Password from 'primevue/password'
import Card from 'primevue/card'
import Message from 'primevue/message'
import Toolbar from 'primevue/toolbar'
import Menu from 'primevue/menu'
import Avatar from 'primevue/avatar'
import Divider from 'primevue/divider'
import Chip from 'primevue/chip'
import FloatLabel from 'primevue/floatlabel'

export function setupPrimeVue(app) {
  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: '.p-dark',
        cssLayer: {
          name: 'primevue',
          order: 'tailwind, primevue',
        },
      },
    },
  })

  // Global component kaydı
  app.component('Button', Button)
  app.component('InputText', InputText)
  app.component('Password', Password)
  app.component('Card', Card)
  app.component('Message', Message)
  app.component('Toolbar', Toolbar)
  app.component('Menu', Menu)
  app.component('Avatar', Avatar)
  app.component('Divider', Divider)
  app.component('Chip', Chip)
  app.component('FloatLabel', FloatLabel)
}
