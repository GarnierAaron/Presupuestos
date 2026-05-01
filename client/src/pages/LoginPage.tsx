import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

function formatApiError(err: unknown): string {
  if (!err || typeof err !== 'object' || !('response' in err))
    return 'No se pudo completar la operación.'
  const data = (err as { response?: { data?: unknown } }).response?.data
  if (data == null) return 'No se pudo completar la operación.'
  if (typeof data === 'string') return data
  if (typeof data === 'object' && data !== null) {
    const o = data as { title?: string; detail?: string }
    const parts = [o.title, o.detail].filter(Boolean)
    if (parts.length) return parts.join(' — ')
  }
  try {
    return JSON.stringify(data)
  } catch {
    return 'Error desconocido.'
  }
}

export function LoginPage() {
  const { login, register } = useAuth()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const forbidden = params.get('forbidden')

  const [isRegister, setIsRegister] = useState(false)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [tenantName, setTenantName] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      if (isRegister) {
        await register({ email, password, tenantName })
      } else {
        await login({ email, password })
      }
      navigate('/', { replace: true })
    } catch (err: unknown) {
      setError(formatApiError(err))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <div className="auth-card">
        <h1>{isRegister ? 'Crear cuenta' : 'Entrar'}</h1>
        {forbidden && (
          <p className="banner banner--warn">
            No tienes permiso para ese recurso o la sesión caducó.
          </p>
        )}
        <form onSubmit={(e) => void onSubmit(e)} className="form">
          {isRegister && (
            <label className="field">
              <span>Organización</span>
              <input
                value={tenantName}
                onChange={(e) => setTenantName(e.target.value)}
                required
                autoComplete="organization"
                placeholder="Ej. Estudio García"
              />
              <span className="field__hint">
                Es el nombre de tu empresa o equipo: aparecerá arriba a la derecha cuando entres (no
                es el correo).
              </span>
            </label>
          )}
          <label className="field">
            <span>Email</span>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              autoComplete="email"
            />
          </label>
          <label className="field">
            <span>Contraseña</span>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              autoComplete={isRegister ? 'new-password' : 'current-password'}
            />
          </label>
          {error && <p className="form-error">{error}</p>}
          <button type="submit" className="btn btn--primary" disabled={loading}>
            {loading ? 'Espera…' : isRegister ? 'Registrarme' : 'Entrar'}
          </button>
        </form>
        <p className="auth-alt">
          <button
            type="button"
            className="link-btn"
            onClick={() => {
              setIsRegister(!isRegister)
              setError('')
            }}
          >
            {isRegister ? '¿Ya tienes cuenta? Entrar' : '¿Nuevo? Crear organización'}
          </button>
        </p>
      </div>
    </div>
  )
}
