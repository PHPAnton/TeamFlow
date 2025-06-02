<template>
    <div class="project-select">
        <label for="project">Проект:</label>
        <select id="project"
                :value="modelValue"
                @change="onChange"
                class="form-select">
            <option v-for="project in projects" :key="project.id" :value="project.id">
                {{ project.title }}
            </option>
        </select>
    </div>
</template>

<script setup>
    defineProps({
        modelValue: [String, Number],
        projects: { type: Array, required: true }
    })
    const emit = defineEmits(['update:modelValue'])

    function onChange(e) {
        emit('update:modelValue', e.target.value)
    }
</script>
