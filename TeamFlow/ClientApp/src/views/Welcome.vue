<template>
    <div class="animated-bg">
        <div class="overlay">
            <div class="container text-center text-white">
                <h1 class="display-3 fw-bold mb-4">Добро пожаловать в TeamFlow</h1>
                <p class="lead mb-4">Управляйте задачами и проектами легко и красиво.</p>
                <router-link to="/register" class="btn btn-light btn-lg">Начать</router-link>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .animated-bg {
        position: relative;
        height: 100vh;
        width: 100%;
        overflow: hidden;
        background: radial-gradient(ellipse at top, #1e1e2f 0%, #0f0f1a 100%);
    }

        .animated-bg::before {
            content: "";
            position: absolute;
            top: -50%;
            left: -50%;
            width: 200%;
            height: 200%;
            background: conic-gradient(from 90deg, #6610f2, #0d6efd, #6610f2);
            animation: rotate 20s linear infinite;
            opacity: 0.08;
        }

    @keyframes rotate {
        0% {
            transform: rotate(0deg);
        }

        100% {
            transform: rotate(360deg);
        }
    }

    .overlay {
        position: relative;
        z-index: 1;
        height: 100%;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 2rem;
    }
</style>
