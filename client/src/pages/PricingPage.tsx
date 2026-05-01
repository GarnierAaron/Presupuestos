import axios from 'axios'
import { useCallback, useEffect, useState } from 'react'
import { createSubscription, fetchMySubscription } from '../api/subscriptions'
import type { MySubscriptionResponseDto } from '../types/api'

interface PlanDef {
  id: string
  name: string
  price: string
  features: string[]
}

const PLANS: PlanDef[] = [
  {
    id: 'Free',
    name: 'Free',
    price: '$0 / mes',
    features: ['Gestión básica de insumos', 'Hasta 5 presupuestos por mes', 'Soporte comunitario'],
  },
  {
    id: 'Pro',
    name: 'Pro',
    price: '$15 / mes',
    features: ['Insumos ilimitados', 'Presupuestos ilimitados', 'Soporte prioritario'],
  },
  {
    id: 'Premium',
    name: 'Premium',
    price: '$29 / mes',
    features: ['Todo lo de Pro', 'Múltiples usuarios por organización', 'Reglas de precios flexibles'],
  },
]

function formatDt(iso: string | null | undefined) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleDateString('es-AR')
  } catch {
    return iso
  }
}

export function PricingPage() {
  const [currentSub, setCurrentSub] = useState<MySubscriptionResponseDto | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [subscribingTo, setSubscribingTo] = useState<string | null>(null)

  const loadSubscription = useCallback(async (signal?: AbortSignal) => {
    setError(null)
    setLoading(true)
    try {
      const data = await fetchMySubscription(signal)
      setCurrentSub(data)
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        if (err.code === 'ERR_CANCELED') return
        if (err.response?.status === 404) {
          setCurrentSub(null) // No subscription
        } else {
          setError('Error al cargar el estado de la suscripción.')
        }
      } else {
        setError('Ocurrió un error inesperado.')
      }
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    const ac = new AbortController()
    void loadSubscription(ac.signal)
    return () => ac.abort()
  }, [loadSubscription])

  const handleSubscribe = async (planId: string) => {
    setError(null)
    setSubscribingTo(planId)
    try {
      const res = await createSubscription({ plan: planId })
      if (res.checkoutUrl) {
        window.location.href = res.checkoutUrl
      } else {
        // Para planes gratis (activación inmediata) recargamos los datos
        await loadSubscription()
      }
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || 'Error al iniciar la suscripción.')
      } else {
        setError('Ocurrió un error inesperado al procesar la suscripción.')
      }
    } finally {
      setSubscribingTo(null)
    }
  }

  const getStatusBadge = () => {
    if (!currentSub) return <span className="badge badge--off">Sin suscripción</span>
    
    if (currentSub.status === 'Active') {
      return <span className="badge badge--ok">Activo</span>
    } else if (currentSub.status === 'Pending') {
      return <span className="badge" style={{ background: '#fef08a', color: '#854d0e' }}>Pendiente</span>
    } else {
      return <span className="badge badge--off">{currentSub.status}</span>
    }
  }

  return (
    <div className="page page--pricing">
      <div className="page__head">
        <div>
          <h1>Planes y Facturación</h1>
          <p className="lead" style={{ marginBottom: 0 }}>
            Elegí el plan que mejor se adapte a tu negocio.
          </p>
        </div>
      </div>

      {error ? (
        <div className="banner banner--warn" role="alert">
          {error}
        </div>
      ) : null}

      <div className="card" style={{ padding: '1.25rem', marginBottom: '2rem' }}>
        <h2 style={{ marginTop: 0, fontSize: '1.1rem' }}>Tu estado actual</h2>
        {loading ? (
          <p style={{ margin: 0, color: 'var(--muted)' }}>Cargando...</p>
        ) : (
          <div style={{ display: 'flex', gap: '1.5rem', flexWrap: 'wrap', alignItems: 'center' }}>
            <div>
              <span style={{ fontSize: '0.85rem', color: 'var(--muted)', display: 'block' }}>Plan</span>
              <strong>{currentSub?.plan || 'Ninguno'}</strong>
            </div>
            <div>
              <span style={{ fontSize: '0.85rem', color: 'var(--muted)', display: 'block' }}>Estado</span>
              {getStatusBadge()}
            </div>
            {currentSub?.startDate && (
              <div>
                <span style={{ fontSize: '0.85rem', color: 'var(--muted)', display: 'block' }}>Inicio</span>
                <span>{formatDt(currentSub.startDate)}</span>
              </div>
            )}
            {currentSub?.endDate && (
              <div>
                <span style={{ fontSize: '0.85rem', color: 'var(--muted)', display: 'block' }}>Vencimiento</span>
                <span>{formatDt(currentSub.endDate)}</span>
              </div>
            )}
          </div>
        )}
      </div>

      <div className="pricing-grid">
        {PLANS.map((plan) => (
          <div key={plan.id} className="pricing-card">
            <h3 className="pricing-card__name">{plan.name}</h3>
            <div className="pricing-card__price">{plan.price}</div>
            <ul className="pricing-card__features">
              {plan.features.map((f, i) => (
                <li key={i}>{f}</li>
              ))}
            </ul>
            <div style={{ marginTop: 'auto', paddingTop: '1.5rem' }}>
              <button
                type="button"
                className={`btn pricing-card__btn ${
                  currentSub?.plan === plan.id ? 'btn--ghost' : 'btn--primary'
                }`}
                disabled={subscribingTo !== null || loading || currentSub?.plan === plan.id}
                onClick={() => void handleSubscribe(plan.id)}
              >
                {subscribingTo === plan.id
                  ? 'Procesando...'
                  : currentSub?.plan === plan.id
                  ? 'Plan actual'
                  : 'Suscribirse'}
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  )
}
