<template>
    <div class="register-container">
        <h1>Регистрация TeamFlow</h1>
        <form @submit.prevent="handleRegister">
            <label>Имя пользователя:</label>
            <input type="text" v-model="username" required />

            <label>Email:</label>
            <input type="email" v-model="email" required />

            <label>Пароль:</label>
            <input type="password" v-model="password" required />

            <button type="submit">Зарегестрироваться</button>

            <p class="error" v-if="error">{{ error }}</p>
            <p class="success" v-if="success">{{ success }}</p>
            <router-link to="/login">Уже есть аккаунт? Войти</router-link>
        </form>
    </div>
</template>

<script setup>
    import { ref } from 'vue';
    import api from '@/axios';

    const username = ref('');
    const email = ref('');
    const password = ref('');
    const error = ref('');
    const success = ref('');

    const handleRegister = async () => {
        error.value = '';
        success.value = '';
        try {
            await api.post('/account/register', {
                username: username.value,
                email: email.value,
                password: password.value,
            });
            success.value = 'Письмо с подтверждением отправлено на email';
        } catch (err) {
            error.value =
                err.response?.data?.message ||
                err.response?.data ||
                'Ошибка регистрации';
        }
    };

</script>

<style scoped>
    .register-container {
        max-width: 420px;
        margin: 80px auto;
        padding: 40px;
        background: rgba(30, 30, 47, 0.95);
        border-radius: 10px;
        box-shadow: 0 0 20px rgba(0, 0, 0, 0.5);
        color: #f1f1f1;
        backdrop-filter: blur(10px);
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
        background: #4caf50;
        border: none;
        color: white;
        font-weight: bold;
        border-radius: 4px;
        cursor: pointer;
    }

        button:hover {
            background: #3e8e41;
        }

    .error {
        margin-top: 12px;
        color: #ff6b6b;
        text-align: center;
    }

    .success {
        margin-top: 12px;
        color: #4caf50;
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
