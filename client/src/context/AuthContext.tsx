import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import * as authApi from '../api/auth'
import {
  AUTH_STORAGE_CHANGED_EVENT,
  clearAuthStorage,
  persistSession,
} from '../api/client'
import { getOrCreateDeviceId } from '../lib/deviceId'
import { isSuperAdminAccessToken } from '../lib/jwt'
import type { LoginRequestDto, RegisterRequestDto } from '../types/api'

type AuthContextValue = {
  accessToken: string | null
  tenantId: string | null
  /** Nombre de la organización (tenant); viene del login/refresh. */
  tenantName: string | null
  userId: string | null
  /** Derivado del JWT (claim `super_admin`). */
  isSuperAdmin: boolean
  login: (dto: LoginRequestDto) => Promise<void>
  register: (dto: RegisterRequestDto) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [accessToken, setAccessToken] = useState<string | null>(() =>
    localStorage.getItem('accessToken')
  )
  const [tenantId, setTenantId] = useState<string | null>(() =>
    localStorage.getItem('tenantId')
  )
  const [tenantName, setTenantName] = useState<string | null>(() =>
    localStorage.getItem('tenantName')
  )
  const [userId, setUserId] = useState<string | null>(() =>
    localStorage.getItem('userId')
  )

  useEffect(() => {
    const syncFromStorage = () => {
      try {
        setAccessToken(localStorage.getItem('accessToken'))
        setTenantId(localStorage.getItem('tenantId'))
        setTenantName(localStorage.getItem('tenantName'))
        setUserId(localStorage.getItem('userId'))
      } catch {
        /* private mode */
      }
    }
    window.addEventListener(AUTH_STORAGE_CHANGED_EVENT, syncFromStorage)
    return () => window.removeEventListener(AUTH_STORAGE_CHANGED_EVENT, syncFromStorage)
  }, [])

  const login = useCallback(async (dto: LoginRequestDto) => {
    const deviceId = getOrCreateDeviceId()
    const raw = await authApi.login({
      ...dto,
      deviceId,
      deviceName: dto.deviceName ?? 'Web',
    })
    const data = persistSession(raw)
    setAccessToken(data.accessToken)
    setTenantId(data.tenantId)
    setTenantName(data.tenantName)
    setUserId(data.userId)
  }, [])

  const register = useCallback(async (dto: RegisterRequestDto) => {
    const deviceId = getOrCreateDeviceId()
    const raw = await authApi.register({
      ...dto,
      deviceId,
      deviceName: dto.deviceName ?? 'Web',
    })
    const data = persistSession(raw)
    setAccessToken(data.accessToken)
    setTenantId(data.tenantId)
    setTenantName(data.tenantName)
    setUserId(data.userId)
  }, [])

  const logout = useCallback(async () => {
    const rt = localStorage.getItem('refreshToken')
    try {
      if (rt) await authApi.logout(rt)
    } catch {
      /* ignorar red */
    }
    clearAuthStorage()
    setAccessToken(null)
    setTenantId(null)
    setTenantName(null)
    setUserId(null)
  }, [])

  const isSuperAdmin = useMemo(
    () => isSuperAdminAccessToken(accessToken),
    [accessToken]
  )

  const value = useMemo(
    () => ({
      accessToken,
      tenantId,
      tenantName,
      userId,
      isSuperAdmin,
      login,
      register,
      logout,
    }),
    [accessToken, tenantId, tenantName, userId, isSuperAdmin, login, register, logout]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth debe usarse dentro de AuthProvider')
  return ctx
}
