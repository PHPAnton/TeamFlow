import { ref } from 'vue'
import api from '@/axios'
import { jwtDecode } from 'jwt-decode'

export const token = ref(localStorage.getItem('token') || '')
export const user = ref(JSON.parse(localStorage.getItem('user') || 'null'))

export const auth = {
    user,
    async login(email, password) {
        const res = await api.post('/account/login', { email, password })
        token.value = res.data.token
        localStorage.setItem('token', token.value)
        if (res.data.user) {
            user.value = res.data.user
        } else {
            const payload = jwtDecode(token.value)
            user.value = {
                id: payload.id,
                username: payload.username || '',
                email: payload.email || ''
            }
        }
        localStorage.setItem('user', JSON.stringify(user.value))
    },
    logout() {
        token.value = ''
        user.value = null
        localStorage.removeItem('token')
        localStorage.removeItem('user')
    },
    isAuthenticated() {
        return !!token.value
    },
    init() {
        token.value = localStorage.getItem('token') || ''
        user.value = JSON.parse(localStorage.getItem('user') || 'null')
    }
}
