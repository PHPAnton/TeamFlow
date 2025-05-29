import axios from 'axios';
import { token } from '@/store/authStore'; // ✅ импортируешь token

const api = axios.create({
    baseURL: 'https://localhost:7143/api',
    timeout: 10000,
    headers: {
        'Content-Type': 'application/json',
    },
});

api.interceptors.request.use(
    config => {
        if (token.value) {
            config.headers.Authorization = `Bearer ${token.value}`;
        }
        return config;
    },
    error => Promise.reject(error)
);

api.interceptors.response.use(
    response => response,
    error => {
        if (error.response?.status === 401) {
            alert('Сессия истекла. Пожалуйста, войдите заново.');
            token.value = ''; // ⚠️ сбросим в store
            localStorage.removeItem('token');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export default api;
