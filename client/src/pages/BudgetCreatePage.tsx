import axios from 'axios'
import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import * as budgetsApi from '../api/budgets'
import * as itemsApi from '../api/items'
import * as servicesApi from '../api/services'
import { NumericTextInput } from '../components/NumericTextInput'
import { parseDecimal } from '../lib/numericStrings'
import {
  buildItemCostMap,
  previewLine,
  sumPreviewLines,
} from '../lib/pricing'
import type { BudgetLineInputDto, ItemDto, ServiceDto } from '../types/api'

type DraftLine = {
  key: string
  serviceId: string
  /** Texto libre en el input (evita `type="number"` y el 0 pegajoso). */
  quantityStr: string
  manualOverride: string
}

function newLine(firstServiceId: string): DraftLine {
  return {
    key: crypto.randomUUID(),
    serviceId: firstServiceId,
    quantityStr: '1',
    manualOverride: '',
  }
}

export function BudgetCreatePage() {
  const navigate = useNavigate()
  const [services, setServices] = useState<ServiceDto[]>([])
  const [items, setItems] = useState<ItemDto[]>([])
  const [lines, setLines] = useState<DraftLine[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    let ok = true
    const ac = new AbortController()
    ;(async () => {
      try {
        const [svc, ins] = await Promise.all([
          servicesApi.listServices(ac.signal),
          itemsApi.listItems(ac.signal),
        ])
        if (!ok) return
        setServices(svc)
        setItems(ins)
        if (svc.length) {
          setLines([newLine(svc[0].id)])
        }
      } catch (err: unknown) {
        if (!ok) return
        if (axios.isAxiosError(err) && err.code === 'ERR_CANCELED') return
        setError('Error cargando servicios o insumos.')
      } finally {
        if (ok) setLoading(false)
      }
    })()
    return () => {
      ok = false
      ac.abort()
    }
  }, [])

  const itemMap = useMemo(() => buildItemCostMap(items), [items])
  const serviceMap = useMemo(
    () => new Map(services.map((s) => [s.id, s])),
    [services]
  )

  const preview = useMemo(() => {
    const parts: { lineCost: number; linePrice: number }[] = []
    for (const row of lines) {
      const svc = serviceMap.get(row.serviceId)
      if (!svc) continue
      const m =
        row.manualOverride.trim() === ''
          ? null
          : parseDecimal(row.manualOverride)
      const qty = parseDecimal(row.quantityStr) ?? 0
      parts.push(previewLine(svc, itemMap, qty, m))
    }
    return sumPreviewLines(parts)
  }, [lines, serviceMap, itemMap])

  function addRow() {
    const sid = services[0]?.id
    if (!sid) return
    setLines((prev) => [...prev, newLine(sid)])
  }

  function updateRow(key: string, patch: Partial<DraftLine>) {
    setLines((prev) =>
      prev.map((r) => (r.key === key ? { ...r, ...patch } : r))
    )
  }

  function removeRow(key: string) {
    setLines((prev) => (prev.length <= 1 ? prev : prev.filter((r) => r.key !== key)))
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      const payload: BudgetLineInputDto[] = lines.map((r) => ({
        serviceId: r.serviceId,
        quantity: parseDecimal(r.quantityStr) ?? 0,
        manualPriceOverride:
          r.manualOverride.trim() === '' ? null : parseDecimal(r.manualOverride),
      }))
      const created = await budgetsApi.createBudget({ lines: payload })
      navigate('/budgets', { state: { createdId: created.id } })
    } catch (err: unknown) {
      const msg =
        err &&
        typeof err === 'object' &&
        'response' in err &&
        err.response &&
        typeof err.response === 'object' &&
        'data' in err.response
          ? JSON.stringify((err.response as { data: unknown }).data)
          : 'No se pudo crear el presupuesto.'
      setError(msg)
    } finally {
      setSaving(false)
    }
  }

  const fmt = new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    maximumFractionDigits: 2,
  })

  if (loading) return <p className="page">Cargando…</p>

  if (!services.length) {
    return (
      <div className="page">
        <h1>Nuevo presupuesto</h1>
        <p className="banner banner--warn">
          Necesitás al menos un servicio configurado.{' '}
          <Link to="/services/new">Crear servicio</Link>
        </p>
      </div>
    )
  }

  return (
    <div className="page">
      <div className="page__head">
        <h1>Nuevo presupuesto</h1>
        <Link to="/budgets" className="btn btn--ghost">
          Cancelar
        </Link>
      </div>
      <p className="hint">
        Vista previa en tiempo real con margen y precio base del servicio. Si el
        tenant usa precio flexible en el servidor, el total final puede diferir.
      </p>
      {error && <p className="banner banner--warn">{error}</p>}

      <form onSubmit={(e) => void onSubmit(e)} className="budget-form">
        <div className="budget-live card">
          <h2>Totales estimados</h2>
          <dl className="budget-live__stats">
            <div>
              <dt>Costo</dt>
              <dd>{fmt.format(preview.totalCost)}</dd>
            </div>
            <div>
              <dt>Precio</dt>
              <dd className="budget-live__price">{fmt.format(preview.totalPrice)}</dd>
            </div>
          </dl>
        </div>

        <div className="budget-lines">
          {lines.map((row) => (
            <div key={row.key} className="budget-line card">
              <label className="field">
                <span>Servicio</span>
                <select
                  value={row.serviceId}
                  onChange={(e) =>
                    updateRow(row.key, { serviceId: e.target.value })
                  }
                >
                  {services.map((s) => (
                    <option key={s.id} value={s.id}>
                      {s.name}
                    </option>
                  ))}
                </select>
              </label>
              <label className="field">
                <span>Cantidad</span>
                <NumericTextInput
                  variant="decimal"
                  trimIntOnBlur={false}
                  trimDecimalLeadingZerosOnBlur
                  value={row.quantityStr}
                  onChange={(quantityStr) => updateRow(row.key, { quantityStr })}
                />
              </label>
              <label className="field">
                <span>Precio línea manual (total, opcional)</span>
                <NumericTextInput
                  variant="decimal"
                  trimIntOnBlur={false}
                  trimDecimalLeadingZerosOnBlur
                  placeholder="Vacío = calcular"
                  value={row.manualOverride}
                  onChange={(manualOverride) =>
                    updateRow(row.key, { manualOverride })
                  }
                />
              </label>
              <button
                type="button"
                className="btn btn--small btn--ghost"
                onClick={() => removeRow(row.key)}
                disabled={lines.length <= 1}
              >
                Quitar línea
              </button>
            </div>
          ))}
        </div>

        <button type="button" className="btn btn--ghost" onClick={addRow}>
          + Línea
        </button>

        <div className="form__actions">
          <button type="submit" className="btn btn--primary" disabled={saving}>
            {saving ? 'Guardando…' : 'Guardar presupuesto'}
          </button>
        </div>
      </form>
    </div>
  )
}
