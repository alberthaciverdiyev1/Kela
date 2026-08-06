<template>
    <div>
        <FloatLabel>
            <Password
                :id="id"
                :model-value="modelValue"
                @update:model-value="$emit('update:modelValue', $event)"
                class="w-full"
                :feedback="false"
                toggle-mask
                :input-props="{ class: 'w-full' }"
                :invalid="error !== ''"
                @blur="$emit('blur')"
            />
            <label :for="id">{{ label }}</label>
        </FloatLabel>
        <small v-if="error" class="mt-1 text-error-500 hover:pointer:cursor">{{ error }}</small>
    </div>
</template>

<script setup>
defineProps({
    modelValue: {type: String, default: ''},
    label: {type: String, required: true},
    id: {type: String, required: true},
    error: {type: String, default: ''},
})
defineEmits(['update:modelValue', 'blur'])
</script>
