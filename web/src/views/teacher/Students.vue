<template>
  <div class="space-y-6">
    <!-- Durum mesajları -->
    <Toast />

    <!-- ════ ÜST BAR: başlık + Yeni Öğrenci (modal açar) ════ -->
    <div class="flex items-start justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl font-bold text-surface-800 mb-1">Öğrenciler</h1>
        <p class="text-surface-500">Öğrenci oluştur — sistem mail ve şifreyi üretir, öğrenciye iletirsin.</p>
      </div>
      <Button label="Yeni Öğrenci" icon="pi pi-user-plus" @click="openCreateDialog" />
    </div>

    <!-- ════ ÖĞRENCİ LİSTESİ ════ -->
    <Card class="border-none shadow rounded-2xl">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-users text-primary"></i>
          <span>Öğrenci Listesi</span>
        </div>
      </template>
      <template #content>
        <DataTable
          :value="students"
          :loading="loading"
          paginator
          :rows="pageSize"
          :total-records="totalCount"
          :lazy="true"
          :first="first"
          @page="onPage"
          size="small"
          striped-rows
        >
          <Column field="firstName" header="Ad" :sortable="true" />
          <Column field="lastName" header="Soyad" :sortable="true" />
          <Column field="phoneNumber" header="Telefon" />
          <Column field="email" header="E-posta" />
          <Column field="birthDate" header="Doğum Tarihi">
            <template #body="{ data }">
              {{ formatDate(data.birthDate) }}
            </template>
          </Column>
          <Column field="cityName" header="Şehir" />
          <Column header="" :style="{ width: '90px' }">
            <template #body="{ data }">
              <Button
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                size="small"
                :loading="deletingId === data.id"
                @click="remove(data)"
              />
            </template>
          </Column>
          <template #empty>
            <div class="text-center text-surface-400 py-8">
              Henüz öğrenci yok. Sağ üstteki "Yeni Öğrenci" ile ilk öğrenciyi oluştur.
            </div>
          </template>
        </DataTable>
      </template>
    </Card>

    <!-- ════ YENİ ÖĞRENCİ MODALI — Login input stilleri ════ -->
    <Dialog v-model:visible="showCreateDialog" modal header="Yeni Öğrenci" :style="{ width: '440px' }">
      <form @submit.prevent="submit" class="flex flex-col gap-5 pt-1">
        <TextInput
          id="firstName"
          v-model="form.firstName"
          label="Ad *"
          :error="errors.firstName"
          autocomplete="off"
          @blur="errors.firstName = form.firstName.trim() ? '' : 'Ad zorunludur.'"
        />
        <TextInput
          id="phoneNumber"
          v-model="form.phoneNumber"
          label="Telefon *"
          type="tel"
          autocomplete="tel"
          :error="errors.phoneNumber"
          @blur="errors.phoneNumber = form.phoneNumber.trim() ? '' : 'Telefon zorunludur.'"
        />
        <TextInput
          id="email"
          v-model="form.email"
          label="E-posta (opsiyonel)"
          type="email"
          autocomplete="off"
          :error="errors.email"
          @blur="validateEmail"
        />
        <TextInput
          id="lastName"
          v-model="form.lastName"
          label="Soyad (opsiyonel)"
          autocomplete="off"
        />

        <Message severity="info" :closable="false" class="!text-xs">
          E-posta boş bırakılırsa sistem otomatik üretir. Mail ve şifre oluşturma sonrası tek ekranda gösterilir.
        </Message>

        <div class="flex justify-end gap-2 pt-1">
          <Button
            label="Vazgeç"
            icon="pi pi-times"
            text
            severity="secondary"
            type="button"
            :disabled="saving"
            @click="showCreateDialog = false"
          />
          <Button label="Öğrenci Oluştur" icon="pi pi-user-plus" :loading="saving" type="submit" />
        </div>
      </form>
    </Dialog>

    <!-- ════ MAİL + ŞİFRE DIALOG — TEK KOPYALA BUTONU ════ -->
    <Dialog v-model:visible="showCredentials" modal header="Öğrenci Oluşturuldu" :style="{ width: '460px' }">
      <div class="space-y-4">
        <Message severity="success" :closable="false" class="!text-sm">
          Öğrenci başarıyla oluşturuldu. Giriş bilgilerini <strong>tek tıkla kopyala</strong> ve öğrenciye ilet.
        </Message>

        <div class="rounded-xl border border-surface-200 p-4">
          <div class="text-xs text-surface-400 mb-2">E-posta | Şifre</div>
          <div class="flex items-center gap-2">
            <InputText :model-value="credentialsText" readonly class="w-full font-mono text-sm" />
            <Button
              icon="pi pi-copy"
              text
              rounded
              severity="info"
              title="Kopyala"
              @click="copyCredentials"
            />
          </div>
        </div>

        <Message severity="warn" :closable="false" class="!text-sm">
          Şifre yalnızca bu ekranda bir kez gösterilir. Sonradan görüntülenemez.
        </Message>

        <div class="flex justify-end gap-2 pt-1">
          <Button label="Tamam" icon="pi pi-check" @click="showCredentials = false" />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { studentsApi } from '../../api/students'
import TextInput from '../../components/ui/inputs/TextInput.vue'

const toast = useToast()

// ── Modal durumu ──
const showCreateDialog = ref(false)

// ── Form durumu ──
const form = reactive({
  firstName: '',
  lastName: '',
  phoneNumber: '',
  email: '',
})
const errors = reactive({ firstName: '', phoneNumber: '', email: '' })

const saving = ref(false)

// ── Liste ──
const students = ref([])
const loading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const pageSize = 10
const first = ref(0)
const deletingId = ref(null)

// ── Oluşturma sonucu ──
const showCredentials = ref(false)
const createdCredentials = ref(null)

// Tek kopyala → "mail | şifre"
const credentialsText = computed(() => {
  if (!createdCredentials.value) return ''
  return `${createdCredentials.value.email} | ${createdCredentials.value.password}`
})

onMounted(loadStudents)

function openCreateDialog() {
  resetForm()
  showCreateDialog.value = true
}

async function loadStudents() {
  loading.value = true
  try {
    const res = await studentsApi.getPage(page.value, pageSize, 'tr')
    students.value = res.items
    totalCount.value = res.totalCount
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Hata', detail: e.message, life: 4000 })
  } finally {
    loading.value = false
  }
}

function onPage(ev) {
  page.value = ev.page + 1
  first.value = ev.first
  loadStudents()
}

function validateEmail() {
  const email = form.email.trim()
  errors.email = email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) ? 'Geçerli bir e-posta adresi girin.' : ''
}

function validate() {
  errors.firstName = form.firstName.trim() ? '' : 'Ad zorunludur.'
  errors.phoneNumber = form.phoneNumber.trim() ? '' : 'Telefon zorunludur.'
  validateEmail()
  return !errors.firstName && !errors.phoneNumber && !errors.email
}

async function submit() {
  if (!validate()) return

  saving.value = true
  try {
    const payload = {
      firstName: form.firstName.trim(),
      lastName: form.lastName.trim() || null,
      phoneNumber: form.phoneNumber.trim(),
      email: form.email.trim() || null,
    }
    const created = await studentsApi.create(payload)

    // Sistem ürettiği mail + şifreyi göster (tek copy ile)
    createdCredentials.value = created
    showCreateDialog.value = false
    showCredentials.value = true

    resetForm()
    await loadStudents()
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Oluşturulamadı', detail: e.message, life: 5000 })
  } finally {
    saving.value = false
  }
}

function resetForm() {
  form.firstName = ''
  form.lastName = ''
  form.phoneNumber = ''
  form.email = ''
  errors.firstName = ''
  errors.phoneNumber = ''
  errors.email = ''
}

async function remove(student) {
  if (!confirm(`${student.firstName} ${student.lastName || ''} öğrencisini silmek istediğine emin misin?`.trim())) return
  deletingId.value = student.id
  try {
    await studentsApi.remove(student.id)
    toast.add({ severity: 'success', summary: 'Silindi', detail: 'Öğrenci silindi.', life: 3000 })
    await loadStudents()
  } catch (e) {
    toast.add({ severity: 'error', summary: 'Hata', detail: e.message, life: 4000 })
  } finally {
    deletingId.value = null
  }
}

function formatDate(value) {
  if (!value) return '—'
  const parts = String(value).split('-')
  if (parts.length === 3) return `${parts[2]}/${parts[1]}/${parts[0]}`
  return value
}

async function copyCredentials() {
  try {
    await navigator.clipboard.writeText(credentialsText.value)
    toast.add({ severity: 'success', summary: 'Kopyalandı', detail: 'Mail ve şifre panoya kopyalandı.', life: 2500 })
  } catch {
    toast.add({ severity: 'warn', summary: 'Kopyalanamadı', detail: 'Manuel kopyala.', life: 3000 })
  }
}
</script>
