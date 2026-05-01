import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

/** Solo sesión con claim `super_admin` en el JWT (emitido por la API). */
export function SuperAdminRoute() {
  const { accessToken, isSuperAdmin } = useAuth()

  if (!accessToken) {
    return <Navigate to="/login" replace />
  }

  if (!isSuperAdmin) {
    return <Navigate to="/" replace />
  }

  return <Outlet />
}
