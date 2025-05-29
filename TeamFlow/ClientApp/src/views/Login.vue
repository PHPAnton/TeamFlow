<template>
    <div class="login-container">
        <h1>Вход в TeamFlow</h1>
        <form @submit.prevent="handleLogin">
            <label>Email:</label>
            <input type="email" v-model="email" required />

            <label>Пароль:</label>
            <input type="password" v-model="password" required />

            <button type="submit">Войти</button>

            <p class="error" v-if="error">{{ error }}</p>
            <router-link to="/reset">Забыли пароль?</router-link>
            <router-link to="/register">Нет аккаунта? Регистрация</router-link>
        </form>
    </div>
</template>

<script>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { auth } from '@/store/authStore';

export default {
  setup() {
    const email = ref('');
    const password = ref('');
    const error = ref('');
        const router = useRouter();


    const handleLogin = async () => {
      try {
        error.value = '';
        await auth.login(email.value, password.value);
        router.push('/tasks'); // или на /dashboard
      } catch (err) {
        error.value = err.response?.data || 'Ошибка входа';
      }
    };

    return { email, password, error, handleLogin };
  },
};
</script>

<style scoped>
    .login-container {
        max-width: 400px;
        margin: 60px auto;
        padding: 30px;
        background: #1e1e2f;
        border-radius: 8px;
        box-shadow: 0 0 10px #00000033;
    }

    h1 {
        text-align: center;
        margin-bottom: 20px;
    }

    label {
        display: block;
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
        background: #4a90e2;
        border: none;
        color: white;
        font-weight: bold;
        border-radius: 4px;
        cursor: pointer;
    }

        button:hover {
            background: #357ab9;
        }

    .error {
        margin-top: 12px;
        color: #ff6b6b;
        text-align: center;
    }

    a {
        display: block;
        margin-top: 10px;
        text-align: center;
        color: #4a90e2;
        text-decoration: none;
    }
</style>
