/// <reference types="vite/client" />
/// <reference types="vite-plugin-pwa/client" />

interface ImportMetaEnv {
  readonly VITE_API_BASE?: string
  readonly VITE_APP_VERSION?: string
  /** Desarrollo: "true" = bloquear la app si falla /api/app-config (comportamiento estricto). */
  readonly VITE_STRICT_KILL_SWITCH?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
