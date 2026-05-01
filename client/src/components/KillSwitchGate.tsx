import axios from 'axios'
import { useEffect, useState, type ReactNode } from 'react'
import { fetchAppConfig } from '../api/appConfig'

type Props = { children: ReactNode }

/** Solo desarrollo: si es true, no se permite entrar aunque falle la red (probar kill switch real). */
const strictDev =
  import.meta.env.DEV && import.meta.env.VITE_STRICT_KILL_SWITCH === 'true'

function axiosErrMsg(err: unknown): string {
  if (!axios.isAxiosError(err)) {
    return err instanceof Error ? err.message : String(err)
  }
  if (err.code === 'ERR_NETWORK') {
    return 'Sin red o el API no responde en :5279 (¿`dotnet run` en el proyecto Api?).'
  }
  if (err.response) {
    return `HTTP ${err.response.status}: ${JSON.stringify(err.response.data)}`
  }
  return err.message
}

export function KillSwitchGate({ children }: Props) {
  const [phase, setPhase] = useState<'loading' | 'ok' | 'blocked'>('loading')
  const [message, setMessage] = useState('')
  /** Solo DEV: seguimos sin poder verificar app-config (API caído / proxy). */
  const [devSkippedCheck, setDevSkippedCheck] = useState(false)

  useEffect(() => {
    let cancelled = false
    ;(async () => {
      try {
        const dto = await fetchAppConfig()
        if (cancelled) return
        if (dto.blocked || !dto.appEnabled || dto.maintenanceMode) {
          setPhase('blocked')
          setMessage(
            dto.message ||
              'La aplicación no está disponible en este momento.'
          )
          return
        }
        setPhase('ok')
      } catch (err) {
        if (cancelled) return

        if (import.meta.env.DEV && !strictDev) {
          console.warn(
            '[KillSwitch] No se pudo llamar a /api/app-config. En desarrollo se permite continuar. Activa VITE_STRICT_KILL_SWITCH=true para bloquear.',
            err
          )
          setDevSkippedCheck(true)
          setPhase('ok')
          return
        }

        setPhase('blocked')
        const detail =
          import.meta.env.DEV && err != null ? ` ${axiosErrMsg(err)}` : ''
        setMessage(
          'No se pudo comprobar el estado de la aplicación. Revisa tu conexión.' +
            detail
        )
      }
    })()
    return () => {
      cancelled = true
    }
  }, [])

  if (phase === 'loading') {
    return (
      <div className="gate gate--loading">
        <p>Comprobando configuración…</p>
      </div>
    )
  }

  if (phase === 'blocked') {
    return (
      <div className="gate gate--blocked">
        <h1>No disponible</h1>
        <p>{message}</p>
        {import.meta.env.DEV && (
          <p className="gate__hint">
            Arrancá el API: en la raíz del repo{' '}
            <code>dotnet run --project src/Presupuestos.Api</code> (puerto típico{' '}
            <code>5279</code>). El proxy de Vite reenvía <code>/api</code> ahí.
          </p>
        )}
      </div>
    )
  }

  return (
    <>
      {devSkippedCheck && (
        <div className="dev-kill-switch-banner" role="status">
          <strong>Modo desarrollo:</strong> no se pudo contactar{' '}
          <code>/api/app-config</code>. La app sigue; revisá que el API esté en{' '}
          <code>http://localhost:5279</code>.
        </div>
      )}
      {children}
    </>
  )
}
