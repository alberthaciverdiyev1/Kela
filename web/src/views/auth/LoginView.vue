<template>
  <Card class="border-none shadow-2xl rounded-3xl overflow-hidden">
    <template #content>
      <!-- Üst bant -->
      <div class="bg-primary text-white text-center py-8 rounded-3xl rounded-b-none -mt-6 -mx-6 mb-6">
        <div class="mx-auto w-16 h-16 bg-white/20 rounded-2xl flex items-center justify-center mb-3">
          <i class="pi pi-book text-3xl"></i>
        </div>
        <h1 class="text-2xl font-bold m-0">Kela LMS</h1>
        <p class="text-white/80 m-0 mt-1 text-sm">Hesabınıza giriş yapın</p>
      </div>

      <Message v-if="errorMessage" severity="error" class="mb-4" :closable="true" @close="errorMessage = ''">
        {{ errorMessage }}
      </Message>

      <form @submit.prevent="submit" class="flex flex-col gap-5">
        <!-- E-posta -->
        <div>
          <FloatLabel>
            <InputText
              id="email"
              v-model="email"
              class="w-full"
              type="email"
              autocomplete="username"
              :invalid="emailError !== ''"
              @blur="emailError = email.trim() ? '' : 'E-posta zorunludur.'"
            />
            <label for="email">E-posta</label>
          </FloatLabel>
          <small v-if="emailError" class="text-red-500">{{ emailError }}</small>
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
              @blur="passwordError = password ? '' : 'Şifre zorunludur.'"
            />
            <label for="password">Şifre</label>
          </FloatLabel>
          <small v-if="passwordError" class="text-red-500">{{ passwordError }}</small>
        </div>

        <Button
          type="submit"
          label="Giriş Yap"
          icon="pi pi-sign-in"
          icon-pos="right"
          class="w-full justify-center py-3"
          :loading="auth.loading"
        />
      </form>

      <Divider class="my-6">
        <span class="text-sm text-surface-400">veya</span>
      </Divider>

      <div class="text-center text-sm">
        <span class="text-surface-500">Hesabınız yok mu?</span>
        <router-link :to="{ name: 'register' }" class="text-primary font-semibold ml-1">
          Kayıt Ol
        </router-link>
      </div>
    </template>
  </Card>
</template>

<script setup>
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/auth'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const email = ref('')
const password = ref('')
const emailError = ref('')
const passwordError = ref('')
const errorMessage = ref('')

function validate() {
  emailError.value = email.value.trim() ? '' : 'E-posta zorunludur.'
  passwordError.value = password.value ? '' : 'Şifre zorunludur.'
  return !emailError.value && !passwordError.value
}

async function submit() {
  if (!validate()) return

  errorMessage.value = ''
  const result = await auth.login({ email: email.value.trim(), password: password.value })

  if (result.ok) {
    router.push(route.query.redirect || { name: 'dashboard' })
  } else {
    errorMessage.value = result.message
  }
}
</script>
