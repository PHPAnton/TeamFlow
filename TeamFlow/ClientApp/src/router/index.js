import { createRouter, createWebHistory } from 'vue-router';
import Welcome from '@/views/Welcome.vue';
import Login from '@/views/Login.vue';
import Register from '@/views/Register.vue';
import ConfirmEmail from '@/views/ConfirmEmail.vue';
import ResetRequest from '@/views/ResetRequest.vue';
import ResetPassword from '@/views/ResetPassword.vue';
import Tasks from '@/views/Tasks.vue';
import Invite from '@/views/Invite.vue'; // новый импорт
import { auth } from '@/store/authStore';


const routes = [
    { path: '/', component: Welcome },
    { path: '/login', component: Login },
    { path: '/register', component: Register },
    { path: '/confirm', component: ConfirmEmail },
    { path: '/reset', component: ResetRequest },
    { path: '/reset-password', component: ResetPassword },
    { path: '/tasks', component: Tasks, meta: { requiresAuth: true } },
    { path: '/invite/:inviteId', component: Invite }, // новый роут для приглашения
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

// Глобальный навигейшн гард для приватных страниц
router.beforeEach((to, from, next) => {
    // Страницу приглашения пропускаем всегда!
    if (to.path.startsWith('/invite')) {
        next();
        return;
    }
    if (to.meta.requiresAuth && !auth.isAuthenticated()) {
        next('/login');
    } else {
        next();
    }
});

export default router;
