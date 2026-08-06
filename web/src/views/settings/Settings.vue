<template>
  <div class="max-w-4xl mx-auto">
    <PageHeader
      :title="i18n.t('settings.title')"
      :subtitle="i18n.t('settings.subtitle')"
    />

    <!-- Durum mesajları -->
    <Message v-if="config.hasUnsavedChanges" severity="warn" :closable="false" class="mb-4">
      {{ i18n.t('settings.unsaved') }}
    </Message>
    <Message v-if="config.error" severity="error" :closable="true" class="mb-4" @close="config.error = ''">
      {{ config.error }}
    </Message>
    <Message v-if="savedMessage" severity="success" :closable="true" class="mb-4" @close="savedMessage = ''">
      {{ savedMessage }}
    </Message>

    <!-- ════════ GENEL — Site adı ════════ -->
    <Card class="mb-4">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-globe text-primary"></i>
          <span>{{ i18n.t('settings.general') }}</span>
        </div>
      </template>
      <template #content>
        <label class="block text-sm font-medium text-surface-700 mb-2">{{ i18n.t('settings.siteName') }}</label>
        <InputText
          :model-value="config.siteName"
          @update:model-value="config.setSiteName($event)"
          class="w-full sm:w-72"
          maxlength="50"
          placeholder="Kela LMS"
        />
        <p class="text-xs text-surface-400 mt-2">
          {{ i18n.t('settings.siteNameHint') }}
        </p>
      </template>
    </Card>

    <!-- ════════ ARAYÜZ DÜZENİ ════════ -->
    <Card class="mb-4">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-bars text-primary"></i>
          <span>{{ i18n.t('settings.layout') }}</span>
        </div>
      </template>

      <template #content>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
          <!-- Navbar seçeneği -->
          <button
            type="button"
            class="rounded-2xl border-2 p-5 text-left transition-all cursor-pointer"
            :class="optionClass(config.isNavbar)"
            @click="config.setNavMode('navbar')"
          >
            <div class="rounded-lg border border-surface-200 overflow-hidden mb-4">
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
                :class="config.isNavbar ? 'border-primary bg-primary' : 'border-surface-300'"
              >
                <i v-if="config.isNavbar" class="pi pi-check text-white text-xs"></i>
              </span>
              <div>
                <div class="font-semibold text-surface-800">{{ i18n.t('settings.navbarMode') }}</div>
                <div class="text-sm text-surface-500">{{ i18n.t('settings.navbarModeDesc') }}</div>
              </div>
            </div>
          </button>

          <!-- Sidebar seçeneği -->
          <button
            type="button"
            class="rounded-2xl border-2 p-5 text-left transition-all cursor-pointer"
            :class="optionClass(config.isSidebar)"
            @click="config.setNavMode('sidebar')"
          >
            <div class="rounded-lg border border-surface-200 overflow-hidden mb-4">
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
                :class="config.isSidebar ? 'border-primary bg-primary' : 'border-surface-300'"
              >
                <i v-if="config.isSidebar" class="pi pi-check text-white text-xs"></i>
              </span>
              <div>
                <div class="font-semibold text-surface-800">{{ i18n.t('settings.sidebarMode') }}</div>
                <div class="text-sm text-surface-500">{{ i18n.t('settings.sidebarModeDesc') }}</div>
              </div>
            </div>
          </button>
        </div>
      </template>
    </Card>

    <!-- ════════ SİTE RENKLERİ ════════ -->
    <Card class="mb-4">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-palette text-primary"></i>
          <span>{{ i18n.t('settings.colors') }}</span>
        </div>
      </template>

      <template #content>
        <div class="space-y-5">
          <div
            v-for="name in colorNames"
            :key="name"
            class="flex flex-col sm:flex-row sm:items-center gap-3 sm:gap-6 border-bottom-1 border-surface-100 pb-4 last:border-0 last:pb-0"
          >
            <div class="w-40 shrink-0">
              <span class="font-semibold text-surface-800">{{ colorLabel(name) }}</span>
              <div class="text-xs text-surface-400 font-mono">--kela-{{ name }}*</div>
            </div>

            <div class="flex items-center gap-3">
              <input
                type="color"
                :value="config.bases[name]"
                @input="onColorChange(name, $event.target.value)"
                class="w-11 h-11 rounded-lg border border-surface-200 cursor-pointer bg-transparent p-1"
                :aria-label="i18n.t('settings.colorSelect', { name: colorLabel(name) })"
              />
              <InputText
                :model-value="config.bases[name]"
                @update:model-value="onColorChange(name, $event.target.value)"
                class="w-28 font-mono"
                maxlength="7"
              />
            </div>

            <div class="flex items-center gap-1 flex-1 min-w-0">
              <div
                v-for="scale in scales"
                :key="scale"
                class="h-8 flex-1 rounded-md border border-black/5"
                :style="{ backgroundColor: config.colors[name][scale] }"
                :title="`${scale} · ${config.colors[name][scale]}`"
              ></div>
            </div>
          </div>
        </div>
      </template>
    </Card>

    <!-- ════════ CANLI ÖNİZLEME ════════ -->
    <Card class="mb-4">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-eye text-primary"></i>
          <span>{{ i18n.t('settings.preview') }}</span>
        </div>
      </template>
      <template #content>
        <div class="space-y-4">
          <div class="flex flex-wrap gap-2">
            <Button :label="i18n.t('settings.btnPrimary')" />
            <Button :label="i18n.t('settings.btnSecondary')" severity="secondary" />
            <Button :label="i18n.t('settings.btnSuccess')" severity="success" />
            <Button :label="i18n.t('settings.btnWarning')" severity="warning" />
            <Button :label="i18n.t('settings.btnDanger')" severity="danger" />
            <Button :label="i18n.t('settings.btnInfo')" severity="info" />
          </div>
          <div class="flex flex-wrap gap-2">
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-primary-100 text-primary-700">{{ i18n.t('settings.tagPrimary') }}</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-success-100 text-success-700">{{ i18n.t('settings.tagSuccess') }}</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-warning-100 text-warning-700">{{ i18n.t('settings.tagWarning') }}</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-error-100 text-error-700">{{ i18n.t('settings.tagDanger') }}</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-info-100 text-info-700">{{ i18n.t('settings.tagInfo') }}</span>
          </div>
          <div class="flex flex-wrap gap-2">
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-primary">{{ i18n.t('settings.filledPrimary') }}</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-success">{{ i18n.t('settings.filledSuccess') }}</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-warning">{{ i18n.t('settings.filledWarning') }}</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-error">{{ i18n.t('settings.filledDanger') }}</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-info">{{ i18n.t('settings.filledInfo') }}</span>
          </div>
        </div>
      </template>
    </Card>

    <!-- ════════ TEK KAYDET ════════ -->
    <div class="flex items-center gap-3">
      <Button
        :label="i18n.t('settings.save')"
        icon="pi pi-check"
        :disabled="!config.hasUnsavedChanges"
        :loading="config.loading"
        @click="save"
      />
      <Button
        :label="i18n.t('settings.reset')"
        icon="pi pi-refresh"
        text
        severity="secondary"
        :disabled="config.loading"
        @click="config.reset()"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useSiteConfigStore } from '../../stores/siteConfig'
import { useI18n } from '../../stores/i18n'
import { COLOR_NAMES } from '../../theme/tokens'
import PageHeader from '../../components/ui/typography/PageHeader.vue'

const config = useSiteConfigStore()
const i18n = useI18n()

// Renk adı → i18n anahtarı
const COLOR_I18N = {
  primary: 'settings.color.primary',
  secondary: 'settings.color.secondary',
  success: 'settings.color.success',
  warning: 'settings.color.warning',
  error: 'settings.color.error',
  info: 'settings.color.info',
}

const colorNames = COLOR_NAMES
const scales = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900]
const savedMessage = ref('')

function colorLabel(name) {
  return i18n.t(COLOR_I18N[name] ?? name)
}

// Sayfa açıldığında sunucudan en güncel hali çek
onMounted(() => config.refresh())

function onColorChange(name, value) {
  let hex = String(value).trim()
  if (!hex.startsWith('#')) hex = '#' + hex
  if (/^#[0-9a-fA-F]{6}$/.test(hex)) {
    config.setBase(name, hex)
  }
}

// TEK istek: tüm site konfigürasyonu (ad + renkler + düzen) birlikte kaydedilir
async function save() {
  const result = await config.save()
  if (result.ok) {
    savedMessage.value = i18n.t('settings.saved')
  }
}

function optionClass(active) {
  return active
    ? 'border-primary bg-primary-50 shadow-sm'
    : 'border-surface-200 bg-surface-0 hover:border-surface-300'
}
</script>
