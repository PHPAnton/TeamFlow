<template>
    <div id="app">
        <NavBar />
        <router-view />
    </div>
</template>

<script setup>
import NavBar from './components/NavBar.vue'
</script>

<style>
    html, body, #app {
        margin: 0;
        padding: 0;
        height: 100%;
        background: #0f0f1a;
        color: #e0e0e0;
        font-family: 'Inter', sans-serif;
        background-image: url('https://www.transparenttextures.com/patterns/cubes.png');
        background-blend-mode: overlay;
        background-repeat: repeat;
    }
    .modal-overlay {
        position: fixed;
        top: 0;
        left: 0;
        background: rgba(0, 0, 0, 0.75);
        display: flex;
        justify-content: center;
        align-items: center;
        width: 100vw;
        height: 100vh;
        z-index: 9999;
    }

    .modal {
        background: #1e1e2f;
        color: white;
        padding: 30px;
        border-radius: 10px;
        width: 420px;
        max-width: 90%;
        box-shadow: 0 0 15px rgba(0,0,0,0.6);
        z-index: 10000;
    }

</style>
