<template>
  <AuthCard
    icon="pi pi-user-plus"
    :subtitle="i18n.t('auth.registerSubtitle')"
    :footer-prompt="i18n.t('auth.haveAccount')"
    :footer-link-text="i18n.t('auth.login')"
    footer-link-route="login"
  >
    <template #messages>
      <Message v-if="errorMessage" severity="error" class="mb-4" :closable="true" @close="errorMessage = ''">
        {{ errorMessage }}
      </Message>
      <Message v-if="successMessage" severity="success" class="mb-4" :closable="true" @close="successMessage = ''">
        {{ successMessage }}
      </Message>
    </template>

    <form @submit.prevent="submit" class="flex flex-col gap-4">
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <FloatTextInput
          id="firstName"
          v-model="firstName"
          :label="i18n.t('auth.firstName')"
          autocomplete="given-name"
          :error="firstNameError"
          @blur="firstNameError = firstName.trim() ? '' : i18n.t('auth.reqFirstName')"
        />
        <FloatTextInput
          id="lastName"
          v-model="lastName"
          :label="i18n.t('auth.lastName')"
          autocomplete="family-name"
          :error="lastNameError"
          @blur="lastNameError = lastName.trim() ? '' : i18n.t('auth.reqLastName')"
        />
      </div>

      <FloatTextInput
        id="email"
        v-model="email"
        :label="i18n.t('auth.email')"
        type="email"
        autocomplete="email"
        :error="emailError"
        @blur="emailError = email.trim() ? (emailPattern.test(email.trim()) ? '' : i18n.t('auth.invalidEmail')) : i18n.t('auth.reqEmail')"
      />

      <PasswordInput
        id="password"
        v-model="password"
        :label="i18n.t('auth.password')"
        :error="passwordError"
        @blur="passwordError = password ? (password.length >= 6 ? '' : i18n.t('auth.passwordMin')) : i18n.t('auth.reqPassword')"
      />

      <PasswordInput
        id="confirmPassword"
        v-model="confirmPassword"
        :label="i18n.t('auth.passwordConfirm')"
        :error="confirmError"
        @blur="confirmError = confirmPassword ? (confirmPassword === password ? '' : i18n.t('auth.passwordMismatch')) : i18n.t('auth.reqConfirmPassword')"
      />

      <SubmitButton :label="i18n.t('auth.register')" icon="pi pi-check-circle" :loading="auth.loading" />
    </form>
  </AuthCard>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import { useI18n } from '../../stores/i18n'
import AuthCard from '../../components/ui/cards/AuthCard.vue'
import FloatTextInput from '../../components/ui/inputs/FloatTextInput.vue'
import PasswordInput from '../../components/ui/inputs/PasswordInput.vue'
import SubmitButton from '../../components/ui/buttons/SubmitButton.vue'

const auth = useAuthStore()
const i18n = useI18n()
const router = useRouter()

const firstName = ref('')
const lastName = ref('')
const email = ref('')
const password = ref('')
const confirmPassword = ref('')

const firstNameError = ref('')
const lastNameError = ref('')
const emailError = ref('')
const passwordError = ref('')
const confirmError = ref('')

const errorMessage = ref('')
const successMessage = ref('')

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

function validate() {
  firstNameError.value = firstName.value.trim() ? '' : i18n.t('auth.reqFirstName')
  lastNameError.value = lastName.value.trim() ? '' : i18n.t('auth.reqLastName')
  emailError.value = email.value.trim()
    ? emailPattern.test(email.value.trim()) ? '' : i18n.t('auth.invalidEmail')
    : i18n.t('auth.reqEmail')
  passwordError.value = password.value
    ? password.value.length >= 6 ? '' : i18n.t('auth.passwordMin')
    : i18n.t('auth.reqPassword')
  confirmError.value = confirmPassword.value
    ? confirmPassword.value === password.value ? '' : i18n.t('auth.passwordMismatch')
    : i18n.t('auth.reqConfirmPassword')

  return !firstNameError.value && !lastNameError.value && !emailError.value &&
    !passwordError.value && !confirmError.value
}

async function submit() {
  if (!validate()) return

  errorMessage.value = ''
  successMessage.value = ''

  const result = await auth.register({
    firstName: firstName.value.trim(),
    lastName: lastName.value.trim(),
    email: email.value.trim(),
    password: password.value,
  })

  if (result.ok) {
    successMessage.value = i18n.t('auth.registerSuccess')
    setTimeout(() => router.push({ name: 'login' }), 1500)
  } else {
    errorMessage.value = result.message
  }
}
</script>
