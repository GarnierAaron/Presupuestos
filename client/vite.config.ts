import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { VitePWA } from 'vite-plugin-pwa'

// https://vite.dev/config/
export default defineConfig({
  // Rutas relativas: recomendado por Capacitor para cargar assets desde la app nativa.
  // La web en servidor también puede servirse desde la raíz del dominio con esta base.
  base: './',
  plugins: [
    react(),
    VitePWA({
      /** En dev evita que el SW intercepte fetch (especialmente /api vía proxy), típico fallo en el móvil. */
      devOptions: {
        enabled: false,
      },
      registerType: 'autoUpdate',
      includeAssets: ['favicon.svg'],
      manifest: {
        name: 'Presupuestos',
        short_name: 'Presupuestos',
        description: 'Gestión de insumos, servicios y presupuestos',
        theme_color: '#1e3a5f',
        background_color: '#0f172a',
        display: 'standalone',
        lang: 'es',
        start_url: '/',
        icons: [
          {
            src: '/favicon.svg',
            sizes: 'any',
            type: 'image/svg+xml',
            purpose: 'any maskable',
          },
        ],
      },
      workbox: {
        globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
        /** No devolver el index.html de SPA ante rutas /api (build producción / preview LAN). */
        navigateFallbackDenylist: [/^\/api\//],
      },
    }),
  ],
  server: {
    /** Escucha en todas las interfaces para abrir la app desde el celular (misma Wi‑Fi). */
    host: true,
    /** 5173 suele chocar con otro proyecto en esta máquina; 5999 es el puerto de desarrollo acordado. */
    port: 5999,
    /** Si 5999 está ocupado, Vite prueba el siguiente libre. Con strictPort: true fallaría en su lugar. */
    strictPort: false,
    proxy: {
      '/api': {
        target: 'http://localhost:5279',
        changeOrigin: true,
      },
    },
  },
})
