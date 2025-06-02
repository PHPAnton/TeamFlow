<template>
    <div class="confirm-page">
        <h2>Подтверждение email</h2>
        <p v-if="confirmed">✅ Email подтверждён!</p>
        <p v-else-if="error">❌ Ошибка подтверждения.</p>
        <p v-else>⏳ Подтверждение...</p>
    </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/axios'

const route = useRoute()
const confirmed = ref(false)
const error = ref(false)

onMounted(async () => {
  try {
    const token = route.query.token
      await api.post('/account/confirm', { token })
    confirmed.value = true
  } catch {
    error.value = true
  }
})
</script>
