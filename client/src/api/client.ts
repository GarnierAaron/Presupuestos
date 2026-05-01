import axios, {
  AxiosHeaders,
  type AxiosError,
  type InternalAxiosRequestConfig,
} from 'axios'
import type { TokenResponseDto } from '../types/api'

export const apiBase = import.meta.env.VITE_API_BASE ?? ''

/** Disparado tras persistir tokens (p. ej. refresh) para sincronizar React con `localStorage`. */
export const AUTH_STORAGE_CHANGED_EVENT = 'presupuestos-auth-storage-changed'

function notifyAuthStorageChanged() {
  if (typeof window === 'undefined') return
  window.dispatchEvent(new Event(AUTH_STORAGE_CHANGED_EVENT))
}

export const api = axios.create({
  baseURL: apiBase,
  headers: { 'Content-Type': 'application/json' },
})

/** Peticiones sin interceptor de refresh (login, refresh). */
export const plainApi = axios.create({
  baseURL: apiBase,
  headers: { 'Content-Type': 'application/json' },
})

type RetryConfig = InternalAxiosRequestConfig & { _retry?: boolean }

/**
 * Actualizado en el mismo instante que persistTokens (antes de cualquier useEffect hijo).
 * Evita la condición de carrera login → navegar a /services → GET sin Bearer.
 */
let syncAccessToken: string | null = null
let syncTenantId: string | null = null

function readAccessTokenForRequest() {
  try {
    const stored = localStorage.getItem('accessToken')
    if (stored) return stored
  } catch {
    /* private mode */
  }
  return syncAccessToken
}

function readTenantIdForRequest() {
  try {
    const stored = localStorage.getItem('tenantId')
    if (stored) return stored
  } catch {
    /* private mode */
  }
  return syncTenantId
}

export function clearAuthStorage() {
  syncAccessToken = null
  syncTenantId = null
  localStorage.removeItem('accessToken')
  localStorage.removeItem('refreshToken')
  localStorage.removeItem('tenantId')
  localStorage.removeItem('tenantName')
  localStorage.removeItem('userId')
}

function pickStr(
  o: Record<string, unknown>,
  camel: keyof TokenResponseDto | string,
  pascal: string
): string {
  const a = o[String(camel)]
  const b = o[pascal]
  if (typeof a === 'string' && a.length > 0) return a
  if (typeof b === 'string' && b.length > 0) return b
  return ''
}

/** Acepta camelCase (API por defecto) o PascalCase si el servidor no renombra propiedades. */
export function normalizeTokenResponse(raw: unknown): TokenResponseDto {
  if (!raw || typeof raw !== 'object') {
    throw new Error('[Auth] Respuesta de sesión inválida.')
  }
  const o = raw as Record<string, unknown>
  const accessToken = pickStr(o, 'accessToken', 'AccessToken')
  const refreshToken = pickStr(o, 'refreshToken', 'RefreshToken')
  const userId = pickStr(o, 'userId', 'UserId')
  const tenantFromApi = o.tenantId ?? o.TenantId
  const tenantId =
    typeof tenantFromApi === 'string' && tenantFromApi.length > 0 ? tenantFromApi : null
  const tnRaw = o.tenantName ?? o.TenantName
  const tenantName =
    typeof tnRaw === 'string' && tnRaw.trim().length > 0 ? tnRaw.trim() : null
  const accessTokenExpiresAt =
    pickStr(o, 'accessTokenExpiresAt', 'AccessTokenExpiresAt') || new Date().toISOString()
  const refreshTokenExpiresAt =
    pickStr(o, 'refreshTokenExpiresAt', 'RefreshTokenExpiresAt') || new Date().toISOString()
  if (!accessToken || !refreshToken || !userId) {
    throw new Error(
      '[Auth] Faltan datos en la respuesta (accessToken, refreshToken, userId).'
    )
  }
  return {
    accessToken,
    refreshToken,
    accessTokenExpiresAt,
    refreshTokenExpiresAt,
    userId,
    tenantId,
    tenantName,
  }
}

function persistTokens(data: TokenResponseDto) {
  syncAccessToken = data.accessToken
  syncTenantId = data.tenantId
  try {
    localStorage.setItem('accessToken', data.accessToken)
    localStorage.setItem('refreshToken', data.refreshToken)
    if (data.tenantId) {
      localStorage.setItem('tenantId', data.tenantId)
    } else {
      localStorage.removeItem('tenantId')
    }
    if (data.tenantName) {
      localStorage.setItem('tenantName', data.tenantName)
    } else {
      localStorage.removeItem('tenantName')
    }
    localStorage.setItem('userId', data.userId)
  } catch (e) {
    console.warn(
      '[Auth] No se pudo guardar la sesión en localStorage (modo privado / cuota). La sesión sigue en memoria.',
      e
    )
  }
  notifyAuthStorageChanged()
}

export function persistSession(raw: unknown): TokenResponseDto {
  const data = normalizeTokenResponse(raw)
  persistTokens(data)
  return data
}

/** Ruta real /api/Auth/... aunque axios guarde URL absoluta o relativa + baseURL. */
function isAuthRequest(config?: InternalAxiosRequestConfig) {
  if (!config) return false
  let path = config.url ?? ''
  const base = config.baseURL ?? ''
  if (path && !path.includes('://') && (base.startsWith('http://') || base.startsWith('https://'))) {
    try {
      path = new URL(path, base.endsWith('/') ? base : `${base}/`).pathname
    } catch {
      path = `${base}${path}`
    }
  } else if (path.includes('://')) {
    try {
      path = new URL(path).pathname
    } catch {
      /* mantener path tal cual */
    }
  }
  return (
    path.includes('/api/Auth/login') ||
    path.includes('/api/Auth/register') ||
    path.includes('/api/Auth/refresh')
  )
}

api.interceptors.request.use((config) => {
  const headers = AxiosHeaders.from(config.headers ?? {})
  const token = readAccessTokenForRequest()
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  } else if (import.meta.env.DEV && !isAuthRequest(config)) {
    console.warn('[api] Petición autenticada sin accessToken:', config.method, config.url)
  }
  const tenant = readTenantIdForRequest()
  if (tenant) {
    headers.set('X-Tenant-Id', tenant)
  }
  config.headers = headers
  return config
})

let refreshPromise: Promise<string | null> | null = null

async function refreshAccessToken(): Promise<string | null> {
  const rt = localStorage.getItem('refreshToken')
  if (!rt) return null

  if (!refreshPromise) {
    refreshPromise = (async () => {
      try {
        const { data } = await plainApi.post<TokenResponseDto>('/api/Auth/refresh', {
          refreshToken: rt,
        })
        const normalized = normalizeTokenResponse(data)
        persistTokens(normalized)
        return normalized.accessToken
      } catch (e: unknown) {
        // Solo invalidar sesión si el servidor rechaza el refresh; no borrar en red/5xx.
        if (axios.isAxiosError(e)) {
          const s = e.response?.status
          if (s === 401 || s === 403) {
            clearAuthStorage()
          }
        }
        return null
      } finally {
        refreshPromise = null
      }
    })()
  }

  return refreshPromise
}

api.interceptors.response.use(
  (r) => r,
  async (error: AxiosError) => {
    if (axios.isAxiosError(error) && error.code === 'ERR_CANCELED') {
      return Promise.reject(error)
    }
    const status = error.response?.status
    const original = error.config as RetryConfig | undefined
    if (!original) return Promise.reject(error)

    if (status === 403) {
      clearAuthStorage()
      if (!window.location.pathname.startsWith('/login')) {
        window.location.assign('/login?forbidden=1')
      }
      return Promise.reject(error)
    }

    if (status === 401) {
      if (isAuthRequest(original)) {
        return Promise.reject(error)
      }
      if (original._retry) {
        clearAuthStorage()
        if (!window.location.pathname.startsWith('/login')) {
          window.location.assign('/login')
        }
        return Promise.reject(error)
      }

      original._retry = true
      const access = await refreshAccessToken()
      if (access) {
        const h = AxiosHeaders.from(original.headers ?? {})
        h.set('Authorization', `Bearer ${access}`)
        original.headers = h
        return api(original)
      }

      // Sin token nuevo: ir a login solo si ya no hay sesión (p. ej. refresh rechazado).
      // Fallo de red al refrescar: no borrar ni redirigir.
      const stillHasSession =
        Boolean(localStorage.getItem('refreshToken')) || Boolean(readAccessTokenForRequest())
      if (!stillHasSession) {
        clearAuthStorage()
        if (!window.location.pathname.startsWith('/login')) {
          window.location.assign('/login')
        }
      }
      return Promise.reject(error)
    }

    return Promise.reject(error)
  }
)
