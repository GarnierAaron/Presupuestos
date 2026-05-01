/**
 * Lectura mínima del payload JWT (solo UI; la API valida firma y permisos).
 */
export function parseJwtPayload(accessToken: string): Record<string, unknown> | null {
  try {
    const parts = accessToken.split('.')
    if (parts.length < 2) return null
    const b64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const pad = '='.repeat((4 - (b64.length % 4)) % 4)
    const json = atob(b64 + pad)
    return JSON.parse(json) as Record<string, unknown>
  } catch {
    return null
  }
}

/** Claim emitido por la API para super administrador global. */
export function isSuperAdminAccessToken(accessToken: string | null): boolean {
  if (!accessToken) return false
  const payload = parseJwtPayload(accessToken)
  if (!payload) return false
  return String(payload.super_admin).toLowerCase() === 'true'
}
