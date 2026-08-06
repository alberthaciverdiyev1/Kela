<template>
  <Dialog :visible="visible" modal @update:visible="close" :header="i18n.t('students.new')" :style="{ width: '440px' }">
    <form @submit.prevent="submit" class="flex flex-col gap-5 pt-1">
      <FloatTextInput
        id="firstName"
        v-model="form.firstName"
        :label="i18n.t('students.field.firstName')"
        :error="errors.firstName"
        autocomplete="off"
        @blur="errors.firstName = form.firstName.trim() ? '' : i18n.t('students.reqFirstName')"
      />
      <FloatTextInput
        id="phoneNumber"
        v-model="form.phoneNumber"
        :label="i18n.t('students.field.phone')"
        type="tel"
        autocomplete="tel"
        :error="errors.phoneNumber"
        @blur="errors.phoneNumber = form.phoneNumber.trim() ? '' : i18n.t('students.reqPhone')"
      />
      <FloatTextInput
        id="email"
        v-model="form.email"
        :label="i18n.t('students.field.email')"
        type="email"
        autocomplete="off"
        :error="errors.email"
        @blur="validateEmail"
      />
      <FloatTextInput
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
          @click="close"
        />
        <Button :label="i18n.t('students.create')" icon="pi pi-user-plus" :loading="saving" type="submit" />
      </div>
    </form>
  </Dialog>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import { useI18n } from '../../stores/i18n'
import FloatTextInput from '../ui/inputs/FloatTextInput.vue'

const props = defineProps({
  visible: { type: Boolean, default: false },
  saving: { type: Boolean, default: false },
})

const emit = defineEmits(['update:visible', 'submit'])

const i18n = useI18n()

const form = reactive({
  firstName: '',
  lastName: '',
  phoneNumber: '',
  email: '',
})
const errors = reactive({ firstName: '', phoneNumber: '', email: '' })

watch(() => props.visible, (open) => {
  if (open) resetForm()
})

function close() {
  emit('update:visible', false)
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

function submit() {
  if (!validate()) return
  emit('submit', {
    firstName: form.firstName.trim(),
    lastName: form.lastName.trim() || null,
    phoneNumber: form.phoneNumber.trim(),
    email: form.email.trim() || null,
  })
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
</script>
