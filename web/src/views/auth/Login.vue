<template>
    <AuthCard
        icon="pi pi-book"
        subtitle="Hesabınıza giriş yapın"
        footer-prompt="Hesabınız yok mu?"
        footer-link-text="Kayıt Ol"
        footer-link-route="register"
    >
        <template #messages>
            <Message v-if="errorMessage" severity="error" class="mb-4" :closable="true" @close="errorMessage = ''">
                {{ errorMessage }}
            </Message>
        </template>

        <form @submit.prevent="submit" class="flex flex-col gap-5">
            <TextInput
                id="email"
                v-model="email"
                label="E-posta"
                type="email"
                autocomplete="username"
                :error="emailError"
                @blur="emailError = email.trim() ? '' : 'E-posta zorunludur.'"
            />
            <PasswordInput
                id="password"
                v-model="password"
                label="Şifre"
                :error="passwordError"
                @blur="passwordError = password ? '' : 'Şifre zorunludur.'"
            />
            <SubmitButton label="Giriş Yap" icon="pi pi-sign-in" :loading="auth.loading"/>
        </form>
    </AuthCard>
</template>

<script setup>
import {ref} from 'vue'
import {useRoute, useRouter} from 'vue-router'
import {useAuthStore} from '../../stores/auth'
import AuthCard from '../../components/ui/cards/AuthCard.vue'
import TextInput from '../../components/ui/inputs/TextInput.vue'
import PasswordInput from '../../components/ui/inputs/PasswordInput.vue'
import SubmitButton from '../../components/ui/buttons/SubmitButton.vue'

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
    const result = await auth.login({email: email.value.trim(), password: password.value})

    if (result.ok) {
        router.push(route.query.redirect || {name: 'dashboard'})
    } else {
        errorMessage.value = result.message
    }
}
</script>
