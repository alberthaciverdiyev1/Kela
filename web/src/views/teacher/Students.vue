<template>
  <div class="space-y-6">
    <Toast />

    <div class="flex items-start justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl font-bold text-surface-800 mb-1">{{ i18n.t('students.title') }}</h1>
        <p class="text-surface-500">{{ i18n.t('students.subtitle') }}</p>
      </div>
      <Button :label="i18n.t('students.new')" icon="pi pi-user-plus" @click="showCreateDialog = true" />
    </div>

    <StudentsTable
      :students="students"
      :loading="loading"
      :page-size="pageSize"
      :total-records="totalCount"
      :first="first"
      :deleting-id="deletingId"
      @page="onPage"
      @delete="remove"
    />

    <StudentCreateDialog
      v-model:visible="showCreateDialog"
      :saving="saving"
      @submit="handleCreate"
    />

    <StudentCredentialsDialog
      v-model:visible="showCredentials"
      :credentials="createdCredentials"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useToast } from 'primevue/usetoast'
import { studentsApi } from '../../api/students'
import { useI18n } from '../../stores/i18n'
import StudentsTable from '../../components/students/StudentsTable.vue'
import StudentCreateDialog from '../../components/students/StudentCreateDialog.vue'
import StudentCredentialsDialog from '../../components/students/StudentCredentialsDialog.vue'

const toast = useToast()
const i18n = useI18n()

const students = ref([])
const loading = ref(false)
const totalCount = ref(0)
const page = ref(1)
const pageSize = 10
const first = ref(0)
const deletingId = ref(null)

const showCreateDialog = ref(false)
const saving = ref(false)
const showCredentials = ref(false)
const createdCredentials = ref(null)

onMounted(loadStudents)

watch(() => i18n.locale, () => loadStudents())

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

async function handleCreate(payload) {
  saving.value = true
  try {
    const created = await studentsApi.create(payload)
    createdCredentials.value = created
    showCreateDialog.value = false
    showCredentials.value = true
    await loadStudents()
  } catch (e) {
    toast.add({ severity: 'error', summary: i18n.t('students.createFail'), detail: e.message, life: 5000 })
  } finally {
    saving.value = false
  }
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
</script>
