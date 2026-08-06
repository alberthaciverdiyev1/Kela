<template>
  <DataTable
    :rows="students"
    :columns="columns"
    :actions="actions"
    :loading="loading"
    :page-size="pageSize"
    :total-records="totalCount"
    :search-columns="['firstName', 'lastName', 'email', 'phoneNumber']"
    :first="first"
    :loading-fn="isActionLoading"
    :title="i18n.t('students.list')"
    icon="pi pi-users"
    :empty-text="i18n.t('students.empty')"
    @page="$emit('page', $event)"
    @delete="$emit('delete', $event)"
  />
</template>

<script setup>
import { computed } from 'vue'
import { useI18n } from '../../stores/i18n'
import DataTable from '../ui/tables/DataTable.vue'

const props = defineProps({
  students: { type: Array, default: () => [] },
  loading: { type: Boolean, default: false },
  pageSize: { type: Number, default: 10 },
  totalCount: { type: Number, default: 0 },
  first: { type: Number, default: 0 },
  deletingId: { type: Number, default: null },
})

defineEmits(['page', 'delete'])

const i18n = useI18n()

const columns = computed(() => [
  { field: 'firstName', header: 'students.col.firstName', sortable: true },
  { field: 'lastName', header: 'students.col.lastName', sortable: true },
  { field: 'phoneNumber', header: 'students.col.phone' },
  { field: 'email', header: 'students.col.email' },
  { field: 'birthDate', header: 'students.col.birthDate', type: 'date' },
  { field: 'cityName', header: 'students.col.city' },
])

const actions = computed(() => [
  { key: 'delete', event: 'delete', icon: 'pi pi-trash', severity: 'danger' },
])

function isActionLoading(key, data) {
  return key === 'delete' && props.deletingId === data.id
}
</script>
