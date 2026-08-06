<template>
  <div class="max-w-3xl mx-auto">
    <PageHeader
      title="Arayüz Düzeni"
      subtitle="Menünün nasıl görüneceğini seçin. Seçim anında uygulanır ve tarayıcınızda hatırlanır."
    />

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
      <!-- Navbar seçeneği -->
      <button
        type="button"
        class="rounded-2xl border-2 p-5 text-left transition-all cursor-pointer"
        :class="optionClass(settings.isNavbar)"
        @click="select('navbar')"
      >
        <div class="rounded-lg border border-surface-200 overflow-hidden mb-4">
          <!-- mini navbar mockup -->
          <div class="h-9 bg-surface-100 flex items-center justify-between px-3">
            <div class="flex items-center gap-2">
              <div class="w-4 h-4 rounded bg-primary-300"></div>
              <div class="w-14 h-3 rounded bg-primary-200"></div>
            </div>
            <div class="w-10 h-3 rounded bg-surface-300"></div>
          </div>
          <div class="h-24 bg-surface-50 flex items-center justify-center">
            <div class="w-24 h-4 rounded bg-surface-200"></div>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <span class="w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0"
            :class="settings.isNavbar ? 'border-primary bg-primary' : 'border-surface-300'"
          >
            <i v-if="settings.isNavbar" class="pi pi-check text-white text-xs"></i>
          </span>
          <div>
            <div class="font-semibold text-surface-800">Üst Menü (Navbar)</div>
            <div class="text-sm text-surface-500">Sade, yatay üst menü</div>
          </div>
        </div>
      </button>

      <!-- Sidebar seçeneği -->
      <button
        type="button"
        class="rounded-2xl border-2 p-5 text-left transition-all cursor-pointer"
        :class="optionClass(settings.isSidebar)"
        @click="select('sidebar')"
      >
        <div class="rounded-lg border border-surface-200 overflow-hidden mb-4">
          <!-- mini sidebar mockup -->
          <div class="h-36 bg-surface-50 flex">
            <div class="w-1/3 bg-surface-100 p-2 flex flex-col gap-1.5">
              <div class="h-3 rounded bg-primary-300 w-3/4"></div>
              <div class="h-3 rounded bg-surface-300 w-1/2"></div>
              <div class="h-3 rounded bg-surface-300 w-2/3"></div>
            </div>
            <div class="flex-1 flex items-center justify-center">
              <div class="w-20 h-4 rounded bg-surface-200"></div>
            </div>
          </div>
        </div>
        <div class="flex items-center gap-3">
          <span class="w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0"
            :class="settings.isSidebar ? 'border-primary bg-primary' : 'border-surface-300'"
          >
            <i v-if="settings.isSidebar" class="pi pi-check text-white text-xs"></i>
          </span>
          <div>
            <div class="font-semibold text-surface-800">Yan Menü (Sidebar)</div>
            <div class="text-sm text-surface-500">Solda sabit menü</div>
          </div>
        </div>
      </button>
    </div>

    <Message severity="info" :closable="false" class="mt-6">
      <i class="pi pi-info-circle mr-2"></i>
      Bu tercih yalnızca sizin tarayıcınızda saklanır ve giriş yaptığınız her oturumda hatırlanır.
    </Message>
  </div>
</template>

<script setup>
import { useSettingsStore } from '../../stores/settings'
import PageHeader from '../../components/ui/typography/PageHeader.vue'

const settings = useSettingsStore()

function select(mode) {
  settings.setNavMode(mode)
}

function optionClass(active) {
  return active
    ? 'border-primary bg-primary-50 shadow-sm'
    : 'border-surface-200 bg-surface-0 hover:border-surface-300'
}
</script>
