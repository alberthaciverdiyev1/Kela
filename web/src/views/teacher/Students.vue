<template>
  <div class="space-y-6">
    <Toast />

    <div class="flex items-start justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl font-bold text-surface-800 mb-1">{{ i18n.t('students.title') }}</h1>
        <p class="text-surface-500">{{ i18n.t('students.subtitle') }}</p>
      </div>
      <Button :label="i18n.t('students.new')" icon="pi pi-user-plus" @click="openCreateDialog" />
    </div>

    <Card class="border-none shadow rounded-2xl">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-users text-primary"></i>
          <span>{{ i18n.t('students.list') }}</span>
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
          <Column field="firstName" :header="i18n.t('students.col.firstName')" :sortable="true" />
          <Column field="lastName" :header="i18n.t('students.col.lastName')" :sortable="true" />
          <Column field="phoneNumber" :header="i18n.t('students.col.phone')" />
          <Column field="email" :header="i18n.t('students.col.email')" />
          <Column field="birthDate" :header="i18n.t('students.col.birthDate')">
            <template #body="{ data }">
              {{ formatDate(data.birthDate) }}
            </template>
          </Column>
          <Column field="cityName" :header="i18n.t('students.col.city')" />
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
              {{ i18n.t('students.empty') }}
            </div>
          </template>
        </DataTable>
      </template>
    </Card>

    <Dialog v-model:visible="showCreateDialog" modal :header="i18n.t('students.new')" :style="{ width: '440px' }">
      <form @submit.prevent="submit" class="flex flex-col gap-5 pt-1">
        <TextInput
          id="firstName"
          v-model="form.firstName"
          :label="i18n.t('students.field.firstName')"
          :error="errors.firstName"
          autocomplete="off"
          @blur="errors.firstName = form.firstName.trim() ? '' : i18n.t('students.reqFirstName')"
        />
        <TextInput
          id="phoneNumber"
          v-model="form.phoneNumber"
          :label="i18n.t('students.field.phone')"
          type="tel"
          autocomplete="tel"
          :error="errors.phoneNumber"
          @blur="errors.phoneNumber = form.phoneNumber.trim() ? '' : i18n.t('students.reqPhone')"
        />
        <TextInput
          id="email"
          v-model="form.email"
          :label="i18n.t('students.field.email')"
          type="email"
          autocomplete="off"
          :error="errors.email"
          @blur="validateEmail"
        />
        <TextInput
          id="lastName"
          v-model="form.lastName"
          :label="i18n.t('students.field.lastName')"
          autocomplete="off"
        />

        <Message severity="info" :closable="false" class="!text-xs">
          {{ i18n.t('students.info') }}
        </Message>

        <div class="flex justify-end gap-2 pt-1">
          <Button
            :label="i18n.t('common.cancel')"
            icon="pi pi-times"
            text
            severity="secondary"
            type="button"
            :disabled="saving"
            @click="showCreateDialog = false"
          />
          <Button :label="i18n.t('students.create')" icon="pi pi-user-plus" :loading="saving" type="submit" />
        </div>
      </form>
    </Dialog>

    <Dialog v-model:visible="showCredentials" modal :header="i18n.t('students.createdTitle')" :style="{ width: '460px' }">
      <div class="space-y-4">
        <Message severity="success" :closable="false" class="!text-sm">
          {{ i18n.t('students.createdMsg') }}
        </Message>

        <div class="rounded-xl border border-surface-200 p-4">
          <div class="text-xs text-surface-400 mb-2">{{ i18n.t('students.emailPassword') }}</div>
          <div class="flex items-center gap-2">
            <InputText :model-value="credentialsText" readonly class="w-full font-mono text-sm" />
            <Button
              icon="pi pi-copy"
              text
              rounded
              severity="info"
              :title="i18n.t('students.copy')"
              @click="copyCredentials"
            />
          </div>
        </div>

        <Message severity="warn" :closable="false" class="!text-sm">
          {{ i18n.t('students.passwordOnce') }}
        </Message>

        <div class="flex justify-end gap-2 pt-1">
          <Button :label="i18n.t('common.confirm')" icon="pi pi-check" @click="showCredentials = false" />
        </div>
      </div>
    </Dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import { studentsApi } from '../../api/students'
import { useI18n } from '../../stores/i18n'
import TextInput from '../../components/ui/inputs/TextInput.vue'

const toast = useToast()
const i18n = useI18n()

const showCreateDialog = ref(false)

const form = reactive({
  firstName: '',
  lastName: '',
  phoneNumber: '',
  email: '',
})
const errors = reactive({ firstName: '', phoneNumber: '', email: '' })

const saving = ref(false)

const students = ref([])
const loading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const pageSize = 10
const first = ref(0)
const deletingId = ref(null)

const showCredentials = ref(false)
const createdCredentials = ref(null)

const credentialsText = computed(() => {
  if (!createdCredentials.value) return ''
  return `${createdCredentials.value.email} | ${createdCredentials.value.password}`
})

onMounted(loadStudents)

watch(() => i18n.locale, () => loadStudents())

function openCreateDialog() {
  resetForm()
  showCreateDialog.value = true
}

async function loadStudents() {
  loading.value = true
  try {
    const res = await studentsApi.getPage(page.value, pageSize, i18n.locale)
    students.value = res.items
    totalCount.value = res.totalCount
  } catch (e) {
    toast.add({ severity: 'error', summary: i18n.t('common.error'), detail: e.message, life: 4000 })
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
  errors.email = email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email) ? i18n.t('students.invalidEmail') : ''
}

function validate() {
  errors.firstName = form.firstName.trim() ? '' : i18n.t('students.reqFirstName')
  errors.phoneNumber = form.phoneNumber.trim() ? '' : i18n.t('students.reqPhone')
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

    createdCredentials.value = created
    showCreateDialog.value = false
    showCredentials.value = true

    resetForm()
    await loadStudents()
  } catch (e) {
    toast.add({ severity: 'error', summary: i18n.t('students.createFail'), detail: e.message, life: 5000 })
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
  const name = `${student.firstName} ${student.lastName || ''}`.trim()
  if (!confirm(i18n.t('students.deleteConfirm', { name }))) return
  deletingId.value = student.id
  try {
    await studentsApi.remove(student.id)
    toast.add({ severity: 'success', summary: i18n.t('common.confirm'), detail: i18n.t('students.deleted'), life: 3000 })
    await loadStudents()
  } catch (e) {
    toast.add({ severity: 'error', summary: i18n.t('common.error'), detail: e.message, life: 4000 })
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
    toast.add({ severity: 'success', summary: i18n.t('students.copy'), detail: i18n.t('students.copied'), life: 2500 })
  } catch {
    toast.add({ severity: 'warn', summary: i18n.t('students.copy'), detail: i18n.t('students.copyFailed'), life: 3000 })
  }
}
</script>
