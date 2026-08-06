<template>
    <div>
        <FloatLabel>
            <InputText
                :id="id"
                :model-value="modelValue"
                @update:model-value="$emit('update:modelValue', $event)"
                class="w-full"
                :type="type"
                :autocomplete="autocomplete"
                :invalid="error !== ''"
                @blur="$emit('blur')"
            />
            <label :for="id">{{ label }}</label>
        </FloatLabel>
        <small v-if="error" class="mt-1 text-error-500">{{ error }}</small>
    </div>
</template>

<script setup>
defineProps({
    modelValue: {type: String, default: ''},
    label: {type: String, required: true},
    id: {type: String, required: true},
    type: {type: String, default: 'text'},
    autocomplete: {type: String, default: undefined},
    error: {type: String, default: ''},
})
defineEmits(['update:modelValue', 'blur'])
</script>
