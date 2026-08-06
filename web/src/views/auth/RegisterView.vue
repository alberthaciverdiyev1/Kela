<template>
  <Card class="border-none shadow-2xl rounded-3xl overflow-hidden">
    <template #content>
      <div class="bg-primary text-white text-center py-8 rounded-3xl rounded-b-none -mt-6 -mx-6 mb-6">
        <div class="mx-auto w-16 h-16 bg-white/20 rounded-2xl flex items-center justify-center mb-3">
          <i class="pi pi-user-plus text-3xl"></i>
        </div>
        <h1 class="text-2xl font-bold m-0">Kela LMS</h1>
        <p class="text-white/80 m-0 mt-1 text-sm">Yeni hesap oluşturun</p>
      </div>

      <Message v-if="errorMessage" severity="error" class="mb-4" :closable="true" @close="errorMessage = ''">
        {{ errorMessage }}
      </Message>
      <Message v-if="successMessage" severity="success" class="mb-4" :closable="true" @close="successMessage = ''">
        {{ successMessage }}
      </Message>

      <form @submit.prevent="submit" class="flex flex-col gap-4">
        <!-- Ad / Soyad -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <FloatLabel>
              <InputText
                id="firstName"
                v-model="firstName"
                class="w-full"
                autocomplete="given-name"
                :invalid="firstNameError !== ''"
                @blur="firstNameError = firstName.trim() ? '' : 'Ad zorunludur.'"
              />
              <label for="firstName">Ad</label>
            </FloatLabel>
          </div>
          <div>
            <FloatLabel>
              <InputText
                id="lastName"
                v-model="lastName"
                class="w-full"
                autocomplete="family-name"
                :invalid="lastNameError !== ''"
                @blur="lastNameError = lastName.trim() ? '' : 'Soyad zorunludur.'"
              />
              <label for="lastName">Soyad</label>
            </FloatLabel>
          </div>
        </div>

        <!-- E-posta -->
        <div>
          <FloatLabel>
            <InputText
              id="email"
              v-model="email"
              class="w-full"
              type="email"
              autocomplete="email"
              :invalid="emailError !== ''"
              @blur="emailError = email.trim() ? (emailPattern.test(email.trim()) ? '' : 'Geçerli bir e-posta girin.') : 'E-posta zorunludur.'"
            />
            <label for="email">E-posta</label>
          </FloatLabel>
        </div>

        <!-- Şifre -->
        <div>
          <FloatLabel>
            <Password
              id="password"
              v-model="password"
              class="w-full"
              :feedback="false"
              toggle-mask
              :input-props="{ class: 'w-full' }"
              :invalid="passwordError !== ''"
              @blur="passwordError = password ? (password.length >= 6 ? '' : 'Şifre en az 6 karakter olmalıdır.') : 'Şifre zorunludur.'"
            />
            <label for="password">Şifre</label>
          </FloatLabel>
        </div>

        <!-- Şifre Tekrarı -->
        <div>
          <FloatLabel>
            <Password
              id="confirmPassword"
              v-model="confirmPassword"
              class="w-full"
              :feedback="false"
              toggle-mask
              :input-props="{ class: 'w-full' }"
              :invalid="confirmError !== ''"
              @blur="confirmError = confirmPassword ? (confirmPassword === password ? '' : 'Şifreler eşleşmiyor.') : 'Şifre tekrarı zorunludur.'"
            />
            <label for="confirmPassword">Şifre Tekrarı</label>
          </FloatLabel>
        </div>

        <Button
          type="submit"
          label="Kayıt Ol"
          icon="pi pi-check-circle"
          icon-pos="right"
          class="w-full justify-center py-3"
          :loading="auth.loading"
        />
      </form>

      <Divider class="my-6">
        <span class="text-sm text-surface-400">veya</span>
      </Divider>

      <div class="text-center text-sm">
        <span class="text-surface-500">Zaten hesabınız var mı?</span>
        <router-link :to="{ name: 'login' }" class="text-primary font-semibold ml-1">
          Giriş Yap
        </router-link>
      </div>
    </template>
  </Card>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

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
