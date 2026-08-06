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
import IconField from 'primevue/iconfield'
import InputIcon from 'primevue/inputicon'
import Select from 'primevue/select'
import DatePicker from 'primevue/datepicker'
import Dialog from 'primevue/dialog'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import Toast from 'primevue/toast'
import ToastService from 'primevue/toastservice'

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
  app.component('IconField', IconField)
  app.component('InputIcon', InputIcon)
  app.component('Select', Select)
  app.component('DatePicker', DatePicker)
  app.component('Dialog', Dialog)
  app.component('DataTable', DataTable)
  app.component('Column', Column)
  app.component('Tag', Tag)
  app.component('Toast', Toast)
  app.use(ToastService)
}
