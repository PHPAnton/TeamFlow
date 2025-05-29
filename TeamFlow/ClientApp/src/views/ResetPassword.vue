<template>
    <div class="reset-password-container">
        <h1>Установить новый пароль</h1>
        <form v-if="token" @submit.prevent="handleResetPassword">
            <label>Новый пароль:</label>
            <input type="password" v-model="newPassword" required />

            <button type="submit">Сменить пароль</button>

            <p class="success" v-if="success">{{ success }}</p>
            <p class="error" v-if="error">{{ error }}</p>
        </form>
        <p v-else class="error">Недействительная ссылка сброса</p>
        <router-link to="/login">Назад к входу</router-link>
    </div>
</template>

<script>
import { ref, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import api from '@/axios';

export default {
  setup() {
    const route = useRoute();
    const token = ref('');
    const newPassword = ref('');
    const success = ref('');
    const error = ref('');

    onMounted(() => {
      token.value = route.query.token;
    });

    const handleResetPassword = async () => {
      try {
        success.value = '';
        error.value = '';
        await api.post('/account/reset-password', {
          token: token.value,
          newPassword: newPassword.value,
        });
        success.value = 'Пароль успешно изменён!';
      } catch (err) {
        error.value = err.response?.data || 'Ошибка сброса пароля';
      }
    };

    return { token, newPassword, success, error, handleResetPassword };
  },
};
</script>

<style scoped>
    .reset-password-container {
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
        background: #3498db;
        border: none;
        color: white;
        font-weight: bold;
        border-radius: 4px;
        cursor: pointer;
    }

        button:hover {
            background: #2980b9;
        }

    .success {
        margin-top: 15px;
        color: #4caf50;
    }

    .error {
        margin-top: 15px;
        color: #ff6b6b;
    }
</style>
