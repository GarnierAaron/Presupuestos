import axios from 'axios'
import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import * as servicesApi from '../api/services'
import type { ServiceDto } from '../types/api'

export function ServicesPage() {
  const [list, setList] = useState<ServiceDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let ok = true
    const ac = new AbortController()
    ;(async () => {
      try {
        const data = await servicesApi.listServices(ac.signal)
        if (ok) setList(data)
      } catch (err: unknown) {
        if (!ok) return
        if (axios.isAxiosError(err) && err.code === 'ERR_CANCELED') return
        setError('No se pudieron cargar los servicios.')
      } finally {
        if (ok) setLoading(false)
      }
    })()
    return () => {
      ok = false
      ac.abort()
    }
  }, [])

  return (
    <div className="page">
      <div className="page__head">
        <h1>Servicios</h1>
        <Link to="/services/new" className="btn btn--primary">
          Nuevo servicio
        </Link>
      </div>
      {error && <p className="banner banner--warn">{error}</p>}
      {loading ? (
        <p>Cargando…</p>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Margen %</th>
                <th>Precio base</th>
                <th>Insumos</th>
              </tr>
            </thead>
            <tbody>
              {list.map((s) => (
                <tr key={s.id}>
                  <td>
                    <Link to={`/services/${s.id}`} className="link">
                      {s.name}
                    </Link>
                  </td>
                  <td>{s.marginPercent ?? '—'}</td>
                  <td>{s.basePrice ?? '—'}</td>
                  <td>{s.serviceItems?.length ?? 0}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
