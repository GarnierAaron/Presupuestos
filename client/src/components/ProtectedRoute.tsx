import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function ProtectedRoute() {
  const { accessToken } = useAuth()
  const loc = useLocation()

  if (!accessToken) {
    return <Navigate to="/login" replace state={{ from: loc }} />
  }

  return <Outlet />
}
