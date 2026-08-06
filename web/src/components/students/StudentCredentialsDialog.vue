<template>
  <Dialog :visible="visible" modal @update:visible="close" :header="i18n.t('students.createdTitle')" :style="{ width: '460px' }">
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
        <Button :label="i18n.t('common.confirm')" icon="pi pi-check" @click="close" />
      </div>
    </div>
  </Dialog>
</template>

<script setup>
import { computed } from 'vue'
import { useToast } from 'primevue/usetoast'
import { useI18n } from '../../stores/i18n'

const props = defineProps({
  visible: { type: Boolean, default: false },
  credentials: { type: Object, default: null },
})

const emit = defineEmits(['update:visible'])

const i18n = useI18n()
const toast = useToast()

const credentialsText = computed(() => {
  if (!props.credentials) return ''
  return `${props.credentials.email} | ${props.credentials.password}`
})

function close() {
  emit('update:visible', false)
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
