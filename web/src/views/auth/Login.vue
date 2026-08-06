<template>
    <AuthCard
        icon="pi pi-book"
        :subtitle="i18n.t('auth.loginSubtitle')"
        :footer-prompt="i18n.t('auth.noAccount')"
        :footer-link-text="i18n.t('auth.register')"
        footer-link-route="register"
    >
        <template #messages>
            <Message v-if="errorMessage" severity="error" class="mb-4" :closable="true" @close="errorMessage = ''">
                {{ errorMessage }}
            </Message>
        </template>

        <form @submit.prevent="submit" class="flex flex-col gap-5">
            <FloatTextInput
                id="email"
                v-model="email"
                :label="i18n.t('auth.email')"
                type="email"
                autocomplete="username"
                :error="emailError"
                @blur="emailError = email.trim() ? '' : i18n.t('auth.reqEmail')"
            />
            <PasswordInput
                id="password"
                v-model="password"
                :label="i18n.t('auth.password')"
                :error="passwordError"
                @blur="passwordError = password ? '' : i18n.t('auth.reqPassword')"
            />
            <SubmitButton :label="i18n.t('auth.login')" icon="pi pi-sign-in" :loading="auth.loading"/>
        </form>
    </AuthCard>
</template>

<script setup>
import {ref} from 'vue'
import {useRoute, useRouter} from 'vue-router'
import {useAuthStore, homeRouteFor} from '../../stores/auth'
import {useI18n} from '../../stores/i18n'
import AuthCard from '../../components/ui/cards/AuthCard.vue'
import FloatTextInput from '../../components/ui/inputs/FloatTextInput.vue'
import PasswordInput from '../../components/ui/inputs/PasswordInput.vue'
import SubmitButton from '../../components/ui/buttons/SubmitButton.vue'

const auth = useAuthStore()
const i18n = useI18n()
const route = useRoute()
const router = useRouter()

const email = ref('')
const password = ref('')
const emailError = ref('')
const passwordError = ref('')
const errorMessage = ref('')

function validate() {
    emailError.value = email.value.trim() ? '' : i18n.t('auth.reqEmail')
    passwordError.value = password.value ? '' : i18n.t('auth.reqPassword')
    return !emailError.value && !passwordError.value
}

async function submit() {
    if (!validate()) return

    errorMessage.value = ''
    const result = await auth.login({email: email.value.trim(), password: password.value})

    if (result.ok) {
        router.push(route.query.redirect || {name: homeRouteFor(auth.role)})
    } else {
        errorMessage.value = result.message
    }
}
</script>
