import { ref } from 'vue';
import api from '@/axios';

export const token = ref(localStorage.getItem('token') || '');

export const auth = {
    user: null,

    async login(email, password) {
        const res = await api.post('/account/login', { email, password });
        token.value = res.data.token;
        localStorage.setItem('token', token.value);
    },

    logout() {
        token.value = '';
        localStorage.removeItem('token');
    },

    isAuthenticated() {
        return !!token.value;
    }
};
