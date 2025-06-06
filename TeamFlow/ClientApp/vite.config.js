import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import path from 'path'

export default defineConfig({
    plugins: [vue()],
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
        },
    },
    server: {
        proxy: {
            '/api': {
                target: 'https://localhost:5001',
                changeOrigin: true,
                secure: false,
            },
            '/chatHub': {
                target: 'https://localhost:5001',
                ws: true,
                changeOrigin: true,
                secure: false,
            },
        },
    },
    build: {
        outDir: '../wwwroot',
        emptyOutDir: true,
    },
})
