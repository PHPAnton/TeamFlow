<template>
    <div>
        <nav>
            <router-link to="/tasks">Задачи</router-link>
            <router-link to="/login">Вход</router-link>
            <router-link to="/register">Регистрация</router-link>
        </nav>
        <router-view />
    </div>
</template>

<style scoped>
    nav {
        display: flex;
        gap: 20px;
        background: #1e1e2f;
        padding: 10px;
    }

    a {
        color: white;
        text-decoration: none;
    }

    .router-link-exact-active {
        font-weight: bold;
    }
</style>
