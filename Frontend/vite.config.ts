import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
    plugins: [react()],
    server: {
        proxy: {
            // Redireciona chamadas de API
            '/api': {
                target: 'https://localhost:7140', // Verifique a porta HTTPS da sua API
                changeOrigin: true,
                secure: false,
            },
            // Redireciona a conexão do SignalR (incluindo WebSockets)
            '/hubs': {
                target: 'https://localhost:7140',
                ws: true,
                changeOrigin: true,
                secure: false,
            }
        }
    }
})