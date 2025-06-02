<template>
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark border-bottom border-secondary shadow-sm sticky-top">
        <div class="container">
            <router-link class="navbar-brand fw-bold text-light" to="/">TeamFlow</router-link>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav ms-auto">
                    <template v-if="isAuthenticated">
                        <li class="nav-item d-flex align-items-center">
                            <span class="nav-link text-light">Привет, <b>{{ username }}</b>!</span>
                            <button class="btn btn-link nav-link text-danger" @click="logout" style="padding:0 12px;">Выйти</button>
                        </li>
                    </template>
                    <template v-else>
                        <li class="nav-item">
                            <router-link class="nav-link text-light" to="/login">Вход</router-link>
                        </li>
                        <li class="nav-item">
                            <router-link class="nav-link text-light" to="/register">Регистрация</router-link>
                        </li>
                    </template>
                </ul>
            </div>
        </div>
    </nav>
</template>

<script setup>
import { computed } from 'vue'
import { auth } from '@/store/authStore'

const isAuthenticated = computed(() => auth.isAuthenticated())
const username = computed(() => auth.user.value?.username || auth.user.value?.email || 'Пользователь')

function logout() {
    auth.logout()
    window.location.href = '/login'
}
</script>

<style scoped>
    .navbar-nav .nav-link {
        color: #e0e0e0;
        font-weight: 500;
        background-color: transparent !important;
        transition: color 0.3s ease;
    }

        .navbar-nav .nav-link:hover {
            color: #4a90e2;
            background-color: transparent !important;
        }
</style>
