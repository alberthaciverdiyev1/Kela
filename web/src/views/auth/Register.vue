<template>
  <AuthCard
    icon="pi pi-user-plus"
    subtitle="Yeni hesap oluşturun"
    footer-prompt="Zaten hesabınız var mı?"
    footer-link-text="Giriş Yap"
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
        <TextInput
          id="firstName"
          v-model="firstName"
          label="Ad"
          autocomplete="given-name"
          :error="firstNameError"
          @blur="firstNameError = firstName.trim() ? '' : 'Ad zorunludur.'"
        />
        <TextInput
          id="lastName"
          v-model="lastName"
          label="Soyad"
          autocomplete="family-name"
          :error="lastNameError"
          @blur="lastNameError = lastName.trim() ? '' : 'Soyad zorunludur.'"
        />
      </div>

      <TextInput
        id="email"
        v-model="email"
        label="E-posta"
        type="email"
        autocomplete="email"
        :error="emailError"
        @blur="emailError = email.trim() ? (emailPattern.test(email.trim()) ? '' : 'Geçerli bir e-posta girin.') : 'E-posta zorunludur.'"
      />

      <PasswordInput
        id="password"
        v-model="password"
        label="Şifre"
        :error="passwordError"
        @blur="passwordError = password ? (password.length >= 6 ? '' : 'Şifre en az 6 karakter olmalıdır.') : 'Şifre zorunludur.'"
      />

      <PasswordInput
        id="confirmPassword"
        v-model="confirmPassword"
        label="Şifre Tekrarı"
        :error="confirmError"
        @blur="confirmError = confirmPassword ? (confirmPassword === password ? '' : 'Şifreler eşleşmiyor.') : 'Şifre tekrarı zorunludur.'"
      />

      <SubmitButton label="Kayıt Ol" icon="pi pi-check-circle" :loading="auth.loading" />
    </form>
  </AuthCard>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'
import AuthCard from '../../components/ui/cards/AuthCard.vue'
import TextInput from '../../components/ui/inputs/TextInput.vue'
import PasswordInput from '../../components/ui/inputs/PasswordInput.vue'
import SubmitButton from '../../components/ui/buttons/SubmitButton.vue'

const auth = useAuthStore()
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
  firstNameError.value = firstName.value.trim() ? '' : 'Ad zorunludur.'
  lastNameError.value = lastName.value.trim() ? '' : 'Soyad zorunludur.'
  emailError.value = email.value.trim()
    ? emailPattern.test(email.value.trim()) ? '' : 'Geçerli bir e-posta girin.'
    : 'E-posta zorunludur.'
  passwordError.value = password.value
    ? password.value.length >= 6 ? '' : 'Şifre en az 6 karakter olmalıdır.'
    : 'Şifre zorunludur.'
  confirmError.value = confirmPassword.value
    ? confirmPassword.value === password.value ? '' : 'Şifreler eşleşmiyor.'
    : 'Şifre tekrarı zorunludur.'

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
    successMessage.value = 'Kaydınız oluşturuldu. Lütfen giriş yapın.'
    setTimeout(() => router.push({ name: 'login' }), 1500)
  } else {
    errorMessage.value = result.message
  }
}
</script>
