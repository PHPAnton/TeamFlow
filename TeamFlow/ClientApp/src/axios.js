import axios from 'axios';
import { token } from '@/store/authStore'; // ✅ импортируешь token

const api = axios.create({
    baseURL: '/api',
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

export default api;
