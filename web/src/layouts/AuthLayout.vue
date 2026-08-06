<template>
    <div class="auth-bg min-h-screen flex items-center justify-center p-4">
        <div class="absolute top-4 right-4">
            <LanguageSwitcher />
        </div>
        <div class="w-full max-w-md">
            <router-view/>
        </div>
    </div>
</template>

<script setup>
import LanguageSwitcher from '../components/LanguageSwitcher.vue'
</script>

<style scoped>
.auth-bg {
    /* Tema değişkenlerine bağlı: primary rengi değişirse arka plan da değişir */
    background: linear-gradient(
        135deg,
        var(--kela-primary) 0%,
        var(--kela-primary-400) 50%,
        var(--kela-primary-700) 100%
    );
    position: relative;
    overflow: hidden;
}

.auth-bg::before,
.auth-bg::after {
    content: '';
    position: absolute;
    border-radius: 50%;
    background: rgba(255, 255, 255, 0.08);
}

.auth-bg::before {
    width: 400px;
    height: 400px;
    top: -120px;
    left: -120px;
}

.auth-bg::after {
    width: 300px;
    height: 300px;
    bottom: -100px;
    right: -80px;
}
</style>
