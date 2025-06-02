<template>
    <div class="confirm-container">
        <h1>Подтверждение Email</h1>

        <div v-if="loading">Подтверждение...</div>
        <div v-else-if="success" class="success">Email подтверждён! <router-link to="/login">Войти</router-link></div>
        <div v-else class="error">Ошибка подтверждения. Попробуйте позже.</div>
    </div>
</template>

<script setup>
    import { useRoute } from 'vue-router';
    import { ref, onMounted } from 'vue';
    import api from '@/axios';

    const route = useRoute();
    const email = ref(route.query.email || ''); // 👈 показываем адрес
    const success = ref(false);
    const loading = ref(true);

    onMounted(async () => {
        console.log('ConfirmEmail loaded');
        const token = route.query.token;
        console.log('Token:', token);

        if (!token) return;

        try {
            const response = await api.post('/account/confirm', { token });
            console.log('✅ Success:', response.data);
            success.value = true;
        } catch (err) {
            console.error('❌ Confirmation failed:', err);
            success.value = false;
        } finally {
            loading.value = false;
        }
    });


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
