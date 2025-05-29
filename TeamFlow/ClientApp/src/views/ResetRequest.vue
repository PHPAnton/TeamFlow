<template>
    <div class="reset-container">
        <h1>Сброс пароля</h1>
        <form @submit.prevent="handleResetRequest">
            <label>Email:</label>
            <input type="email" v-model="email" required />

            <button type="submit">Отправить ссылку</button>

            <p class="success" v-if="success">{{ success }}</p>
            <p class="error" v-if="error">{{ error }}</p>
            <router-link to="/login">Назад к входу</router-link>
        </form>
    </div>
</template>

<script>
import { ref } from 'vue';
import api from '@/axios';

export default {
  setup() {
    const email = ref('');
    const success = ref('');
    const error = ref('');

    const handleResetRequest = async () => {
      success.value = '';
      error.value = '';
      try {
        await api.post('/account/reset-request', { email: email.value });
        success.value = 'Ссылка на сброс отправлена, проверьте почту.';
      } catch (err) {
        error.value = err.response?.data || 'Ошибка отправки запроса';
      }
    };

    return { email, success, error, handleResetRequest };
  },
};
</script>

<style scoped>
    .reset-container {
        max-width: 400px;
        margin: 60px auto;
        padding: 30px;
        background: #1e1e2f;
        border-radius: 8px;
        box-shadow: 0 0 10px #00000033;
        text-align: center;
    }

    label {
        display: block;
        text-align: left;
        margin-top: 12px;
    }

    input {
        width: 100%;
        padding: 8px;
        margin-top: 4px;
        border-radius: 4px;
        border: none;
        background: #2e2e3e;
        color: white;
    }

    button {
        margin-top: 20px;
        width: 100%;
        padding: 10px;
        background: #f39c12;
        border: none;
        color: white;
        font-weight: bold;
        border-radius: 4px;
        cursor: pointer;
    }

        button:hover {
            background: #e67e22;
        }

    .success {
        color: #4caf50;
        margin-top: 15px;
    }

    .error {
        color: #ff6b6b;
        margin-top: 15px;
    }
</style>
