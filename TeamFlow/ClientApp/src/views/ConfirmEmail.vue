<template>
    <div class="confirm-container">
        <h1>Подтверждение Email</h1>

        <div v-if="loading">Подтверждение...</div>
        <div v-else-if="success" class="success">Email подтверждён! <router-link to="/login">Войти</router-link></div>
        <div v-else class="error">Ошибка подтверждения. Попробуйте позже.</div>
    </div>
</template>

<script>
import { onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';
import api from '@/axios';

export default {
  setup() {
    const route = useRoute();
    const success = ref(false);
    const loading = ref(true);

    onMounted(async () => {
      const token = route.query.token;
      if (!token) {
        loading.value = false;
        return;
      }

      try {
        await api.post('/account/confirm', { token });
        success.value = true;
      } catch (err) {
        success.value = false;
      } finally {
        loading.value = false;
      }
    });

    return { success, loading };
  },
};
</script>

<style scoped>
    .confirm-container {
        max-width: 400px;
        margin: 60px auto;
        padding: 30px;
        background: #1e1e2f;
        border-radius: 8px;
        text-align: center;
    }

    .success {
        color: #4caf50;
        font-weight: bold;
    }

    .error {
        color: #ff6b6b;
        font-weight: bold;
    }
</style>
