import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import * as budgetsApi from '../api/budgets'
import type { BudgetDto } from '../types/api'

export function BudgetsListPage() {
  const [list, setList] = useState<BudgetDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let ok = true
    ;(async () => {
      try {
        const data = await budgetsApi.listBudgets()
        if (ok) setList(data)
      } catch {
        if (ok) setError('No se pudieron cargar los presupuestos.')
      } finally {
        if (ok) setLoading(false)
      }
    })()
    return () => {
      ok = false
    }
  }, [])

  const fmt = new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    maximumFractionDigits: 2,
  })
  const fmtDate = new Intl.DateTimeFormat('es-AR', {
    dateStyle: 'short',
    timeStyle: 'short',
  })

  return (
    <div className="page">
      <div className="page__head">
        <h1>Presupuestos</h1>
        <Link to="/budgets/new" className="btn btn--primary">
          Nuevo
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
                <th>Fecha</th>
                <th>Costo total</th>
                <th>Precio total</th>
                <th>Líneas</th>
              </tr>
            </thead>
            <tbody>
              {list.map((b) => (
                <tr key={b.id}>
                  <td>{fmtDate.format(new Date(b.createdAt))}</td>
                  <td>{fmt.format(b.totalCost)}</td>
                  <td>{fmt.format(b.totalPrice)}</td>
                  <td>{b.details?.length ?? 0}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
