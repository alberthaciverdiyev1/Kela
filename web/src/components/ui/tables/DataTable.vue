<template>
    <Card class="border-none shadow rounded-2xl">
        <template v-if="searchColumns.length > 0" #header>
            <SearchInput></SearchInput>
        </template>
        <template #content>
            <DataTable
                :value="rows"
                :loading="loading"
                :paginator="paginator"
                :rows="pageSize"
                :total-records="totalRecords"
                :lazy="lazy"
                :first="first"
                :size="size"
                :striped-rows="stripedRows"
                :sort-field="sortField"
                :sort-order="sortOrder"
                :search-columns="searchColumns"
                @page="onPage"
                @sort="onSort"
            >
                <Column
                    v-for="col in columns"
                    :key="col.field"
                    :field="col.field"
                    :header="i18n.t(col.header ?? '')"
                    :sortable="!!col.sortable"
                    :style="col.width ? { width: col.width } : undefined"
                >
                    <template v-if="col.formatter || col.type" #body="{ data }">
                        {{ formatCell(col, data) }}
                    </template>
                </Column>

                <Column v-if="actions && actions.length" header="" :style="{ width: actionWidth }">
                    <template #body="{ data }">
                        <div class="flex items-center gap-1 justify-end">
                            <Button
                                v-for="action in actions"
                                :key="action.key"
                                :icon="action.icon"
                                :severity="action.severity"
                                :text="action.text !== false"
                                rounded
                                size="small"
                                :title="action.title ? i18n.t(action.title) : undefined"
                                :loading="isActionLoading(action, data)"
                                @click="$emit(action.event, data)"
                            />
                        </div>
                    </template>
                </Column>

                <template #empty>
                    <div class="text-center text-surface-400 py-8">
                        {{ emptyText }}
                    </div>
                </template>
            </DataTable>
        </template>
    </Card>
</template>

<script setup>
import {useI18n} from '../../../stores/i18n'
import SearchInput from "../inputs/SearchInput.vue";

const props = defineProps({
    rows: {type: Array, default: () => []},
    searchColumns: {type: Array, default: () => []},
    columns: {type: Array, default: () => []},
    actions: {type: Array, default: () => []},
    actionWidth: {type: String, default: '90px'},
    loading: {type: Boolean, default: false},
    paginator: {type: Boolean, default: true},
    pageSize: {type: Number, default: 10},
    totalRecords: {type: Number, default: 0},
    lazy: {type: Boolean, default: true},
    first: {type: Number, default: 0},
    size: {type: String, default: 'small'},
    stripedRows: {type: Boolean, default: true},
    sortField: {type: String, default: null},
    sortOrder: {type: Number, default: null},
    title: {type: String, default: ''},
    icon: {type: String, default: ''},
    emptyText: {type: String, default: ''},
    loadingFn: {type: Function, default: null},
})

const emit = defineEmits(['page', 'sort', 'delete', 'edit', 'view'])
const i18n = useI18n()

function onPage(ev) {
    emit('page', ev)
}

function onSort(ev) {
    emit('sort', ev)
}

function isActionLoading(action, data) {
    return props.loadingFn ? props.loadingFn(action.key, data) : false
}

function formatCell(col, data) {
    const value = data[col.field]
    if (col.formatter) return col.formatter(value, data)
    if (col.type === 'date') return formatDate(value)
    if (col.type === 'dateTime') return formatDateTime(value)
    return value ?? '—'
}

function formatDate(value) {
    if (!value) return '—'
    const parts = String(value).split('-')
    if (parts.length === 3) return `${parts[2]}/${parts[1]}/${parts[0]}`
    return value
}

function formatDateTime(value) {
    if (!value) return '—'
    const d = new Date(value)
    if (isNaN(d)) return value
    return d.toLocaleDateString('tr-TR') + ' ' + d.toLocaleTimeString('tr-TR', {hour: '2-digit', minute: '2-digit'})
}
</script>
