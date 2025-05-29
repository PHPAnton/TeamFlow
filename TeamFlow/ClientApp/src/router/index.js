import { createRouter, createWebHistory } from 'vue-router';
import Login from '@/views/Login.vue';
import Register from '@/views/Register.vue';
import ConfirmEmail from '@/views/ConfirmEmail.vue';
import ResetRequest from '@/views/ResetRequest.vue';
import ResetPassword from '@/views/ResetPassword.vue';
import Tasks from '@/views/Tasks.vue';
import { auth } from '@/store/authStore';

const routes = [
    { path: '/', redirect: '/login' },
    { path: '/login', component: Login },
    { path: '/register', component: Register },
    { path: '/confirm', component: ConfirmEmail },
    { path: '/reset', component: ResetRequest },
    { path: '/reset-password', component: ResetPassword },
    { path: '/tasks', component: Tasks, meta: { requiresAuth: true } },
];

const router = createRouter({
    history: createWebHistory(),
    routes,
});

router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth && !auth.isAuthenticated()) {
        next('/login');
    } else {
        next();
    }
});

export default router;
