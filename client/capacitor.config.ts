import type { CapacitorConfig } from '@capacitor/cli'

/**
 * webDir debe coincidir con la salida de Vite (`dist`).
 * Para live reload en dispositivo (opcional), usa server.url + cleartext en desarrollo.
 */
const config: CapacitorConfig = {
  appId: 'com.presupuestos.app',
  appName: 'Presupuestos',
  webDir: 'dist',
  server: {
    // Producción: omitido (carga bundle embebido).
    // Desarrollo en LAN (opcional): descomenta y pon tu IP + puerto de `npm run dev`
    // url: 'http://192.168.0.10:5999',
    // cleartext: true,
    androidScheme: 'https',
  },
  android: {
    path: 'android',
  },
}

export default config
