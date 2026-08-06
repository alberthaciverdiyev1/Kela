<template>
  <div class="max-w-4xl mx-auto">
    <PageHeader
      title="Site Tasarımı"
      subtitle="Sitenin tamamında kullanılan anlamsal renkler buradan değiştirilir. Bir rengi değiştirmek butonlardan kartlara, formlardan rozetlere kadar tüm siteye anında yansır."
    />

    <!-- Kaydedilmemiş değişiklik uyarısı -->
    <Message v-if="theme.hasUnsavedChanges" severity="warn" :closable="false" class="mb-4">
      Kaydedilmemiş değişiklikler var. Sayfadan çıkarsanız değişiklikler kaybolur.
    </Message>

    <Card class="mb-4">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-palette text-primary"></i>
          <span>Anlamsal Renkler</span>
        </div>
      </template>

      <template #content>
        <div class="space-y-5">
          <div
            v-for="name in colorNames"
            :key="name"
            class="flex flex-col sm:flex-row sm:items-center gap-3 sm:gap-6 border-bottom-1 border-surface-100 pb-4 last:border-0 last:pb-0"
          >
            <!-- Renk adı + açıklama -->
            <div class="w-40 shrink-0">
              <span class="font-semibold text-surface-800">{{ labels[name] }}</span>
              <div class="text-xs text-surface-400 font-mono">--kela-{{ name }}*</div>
            </div>

            <!-- Base renk seçici -->
            <div class="flex items-center gap-3">
              <input
                type="color"
                :value="theme.bases[name]"
                @input="onColorChange(name, $event.target.value)"
                class="w-11 h-11 rounded-lg border border-surface-200 cursor-pointer bg-transparent p-1"
                :aria-label="`${labels[name]} seç`"
              />
              <InputText
                :model-value="theme.bases[name]"
                @update:model-value="onColorChange(name, $event.target.value)"
                class="w-28 font-mono"
                maxlength="7"
              />
            </div>

            <!-- 50-900 palet önizleme -->
            <div class="flex items-center gap-1 flex-1 min-w-0">
              <div
                v-for="scale in scales"
                :key="scale"
                class="h-8 flex-1 rounded-md border border-black/5"
                :style="{ backgroundColor: theme.colors[name][scale] }"
                :title="`${scale} · ${theme.colors[name][scale]}`"
              ></div>
            </div>
          </div>
        </div>
      </template>
    </Card>

    <!-- Önizleme -->
    <Card class="mb-4">
      <template #title>
        <div class="flex items-center gap-2">
          <i class="pi pi-eye text-primary"></i>
          <span>Canlı Önizleme</span>
        </div>
      </template>
      <template #content>
        <div class="space-y-4">
          <div class="flex flex-wrap gap-2">
            <Button label="Ana Buton" />
            <Button label="İkincil" severity="secondary" />
            <Button label="Başarı" severity="success" />
            <Button label="Uyarı" severity="warning" />
            <Button label="Hata" severity="danger" />
            <Button label="Bilgi" severity="info" />
          </div>
          <div class="flex flex-wrap gap-2">
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-primary-100 text-primary-700">Ana Renk</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-success-100 text-success-700">Başarı</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-warning-100 text-warning-700">Uyarı</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-error-100 text-error-700">Hata</span>
            <span class="px-3 py-1 rounded-full text-sm font-medium bg-info-100 text-info-700">Bilgi</span>
          </div>
          <div class="flex flex-wrap gap-2">
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-primary">Filled Ana</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-success">Filled Başarı</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-warning">Filled Uyarı</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-error">Filled Hata</span>
            <span class="px-3 py-1 rounded-md text-white text-sm font-semibold bg-info">Filled Bilgi</span>
          </div>
          <Message severity="success" :closable="false">İşlem başarıyla tamamlandı.</Message>
        </div>
      </template>
    </Card>

    <!-- Aksiyonlar -->
    <div class="flex items-center gap-3">
      <Button
        label="Değişiklikleri Kaydet"
        icon="pi pi-check"
        :disabled="!theme.hasUnsavedChanges"
        @click="saveTheme"
      />
      <Button
        label="Varsayılanlara Dön"
        icon="pi pi-refresh"
        text
        severity="secondary"
        @click="resetTheme"
      />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useThemeStore } from '../../stores/theme'
import { COLOR_NAMES, COLOR_LABELS, DEFAULT_BASES } from '../../theme/tokens'
import PageHeader from '../../components/ui/typography/PageHeader.vue'

const theme = useThemeStore()

const colorNames = COLOR_NAMES
const labels = COLOR_LABELS
const scales = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900]

const savedBases = ref({ ...theme.bases })

function onColorChange(name, value) {
  // Hex formatını düzelt: # koy, 6 hane bekle
  let hex = String(value).trim()
  if (!hex.startsWith('#')) hex = '#' + hex
  if (/^#[0-9a-fA-F]{6}$/.test(hex)) {
    theme.setBase(name, hex)
  }
}

function saveTheme() {
  theme.save()
  savedBases.value = { ...theme.bases }
}

function resetTheme() {
  theme.reset()
  savedBases.value = { ...DEFAULT_BASES }
}
</script>
