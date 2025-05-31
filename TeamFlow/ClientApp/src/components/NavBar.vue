<template>
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark border-bottom border-secondary shadow-sm sticky-top">
        <div class="container">
            <router-link class="navbar-brand fw-bold text-light" to="/">TeamFlow</router-link>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarNav">
                <ul class="navbar-nav ms-auto">
                    <li class="nav-item">
                        <router-link class="nav-link text-light" to="/login">Вход</router-link>
                    </li>
                    <li class="nav-item">
                        <router-link class="nav-link text-light" to="/register">Регистрация</router-link>
                    </li>
                </ul>
            </div>
        </div>
    </nav>
</template>

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
