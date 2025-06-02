<template>
    <div class="invite-page">
        <h2>Приглашение в проект</h2>
        <div v-if="loading">Проверка приглашения...</div>
        <div v-else-if="inviteError" class="error">{{ inviteError }}</div>
        <div v-else>
            <p>Вас пригласили в проект TeamFlow!</p>
            <form @submit.prevent="acceptInvite">
                <div class="mb-3">
                    <label>Email (автоматически):</label>
                    <input type="email" v-model="email" class="form-control" disabled />
                </div>
                <div class="mb-3">
                    <label>Имя:</label>
                    <input type="text" v-model="username" class="form-control" required />
                </div>
                <div class="mb-3">
                    <label>Пароль:</label>
                    <input type="password" v-model="password" class="form-control" required />
                </div>
                <button class="btn btn-primary" :disabled="submitting">Зарегистрироваться и вступить в проект</button>
            </form>
            <div v-if="submitError" class="error">{{ submitError }}</div>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue'
    import { useRoute, useRouter } from 'vue-router'
    import { auth } from '@/store/authStore'

    const route = useRoute()
    const router = useRouter()
    const inviteId = route.params.inviteId

    const email = ref('')
    const username = ref('')
    const password = ref('')
    const loading = ref(true)
    const inviteError = ref('')
    const submitting = ref(false)
    const submitError = ref('')

    onMounted(async () => {
        try {
            const res = await fetch(`/api/invites/${inviteId}`)
            if (!res.ok) throw new Error('Приглашение не найдено или уже использовано')
            const data = await res.json()
            email.value = data.email
            loading.value = false
        } catch (e) {
            inviteError.value = 'Приглашение не найдено или уже использовано.'
            loading.value = false
        }
    })

    async function acceptInvite() {
        submitting.value = true
        submitError.value = ''
        try {
            // 1. Завершаем регистрацию по приглашению
            const response = await fetch(`/api/ProjectMembers/accept-invite/${inviteId}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    username: username.value,
                    password: password.value
                })
            })
            if (!response.ok) throw new Error(await response.text())

            // 2. Автоматически логиним новым пользователем!
            await auth.login(email.value, password.value)

            // 3. Редиректим на задачи
            router.push('/tasks')
        } catch (e) {
            submitError.value = e.message
        }
        submitting.value = false
    }
</script>

<style scoped>
    .invite-page {
        max-width: 400px;
        margin: 48px auto;
        padding: 32px;
        background: #232340;
        color: #fff;
        border-radius: 10px;
    }

    .mb-3 {
        margin-bottom: 18px;
    }

    .error {
        color: #fc7b7b;
        margin-top: 12px;
    }

    .success {
        color: #67ff9c;
        margin-top: 12px;
    }
</style>
