import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { NumericTextInput } from '../components/NumericTextInput'
import * as itemsApi from '../api/items'
import * as servicesApi from '../api/services'
import { parseDecimal } from '../lib/numericStrings'
import type { CreateServiceDto, ItemDto, ServiceDto } from '../types/api'

type Line = { itemId: string; quantityStr: string }

export function ServiceEditPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const isNew = id === 'new'

  const [items, setItems] = useState<ItemDto[]>([])
  const [name, setName] = useState('')
  const [basePrice, setBasePrice] = useState<string>('')
  const [marginPercent, setMarginPercent] = useState<string>('')
  const [lines, setLines] = useState<Line[]>([])
  const [loading, setLoading] = useState(!isNew)
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    let ok = true
    ;(async () => {
      try {
        const insumos = await itemsApi.listItems()
        if (!ok) return
        setItems(insumos)
        if (!isNew && id) {
          const s = await servicesApi.getService(id)
          if (!ok) return
          applyService(s)
        } else if (insumos.length) {
          setLines([{ itemId: insumos[0].id, quantityStr: '1' }])
        }
      } catch {
        if (ok) setError('Error cargando datos.')
      } finally {
        if (ok) setLoading(false)
      }
    })()
    return () => {
      ok = false
    }
  }, [id, isNew])

  function applyService(s: ServiceDto) {
    setName(s.name)
    setBasePrice(s.basePrice != null ? String(s.basePrice) : '')
    setMarginPercent(s.marginPercent != null ? String(s.marginPercent) : '')
    setLines(
      s.serviceItems?.length
        ? s.serviceItems.map((l) => ({
            itemId: l.itemId,
            quantityStr: String(l.quantityUsed),
          }))
        : []
    )
  }

  function addLine() {
    const first = items[0]?.id
    if (!first) return
    setLines((prev) => [...prev, { itemId: first, quantityStr: '1' }])
  }

  function removeLine(i: number) {
    setLines((prev) => prev.filter((_, j) => j !== i))
  }

  function updateLine(i: number, patch: Partial<Line>) {
    setLines((prev) =>
      prev.map((row, j) => (j === i ? { ...row, ...patch } : row))
    )
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault()
    setSaving(true)
    setError('')
    try {
      const body: CreateServiceDto = {
        name: name.trim(),
        basePrice: basePrice === '' ? null : parseDecimal(basePrice),
        marginPercent: marginPercent === '' ? null : parseDecimal(marginPercent),
        serviceItems: lines.map((l) => ({
          itemId: l.itemId,
          quantityUsed: parseDecimal(l.quantityStr) ?? 0,
        })),
      }
      if (isNew) {
        const created = await servicesApi.createService(body)
        navigate(`/services/${created.id}`, { replace: true })
      } else if (id) {
        await servicesApi.updateService(id, body)
        const s = await servicesApi.getService(id)
        applyService(s)
      }
    } catch (err: unknown) {
      const msg =
        err &&
        typeof err === 'object' &&
        'response' in err &&
        err.response &&
        typeof err.response === 'object' &&
        'data' in err.response
          ? JSON.stringify((err.response as { data: unknown }).data)
          : 'No se pudo guardar.'
      setError(msg)
    } finally {
      setSaving(false)
    }
  }

  async function onDelete() {
    if (!id || isNew) return
    if (!confirm('¿Eliminar este servicio?')) return
    try {
      await servicesApi.deleteService(id)
      navigate('/services')
    } catch {
      setError('No se pudo eliminar.')
    }
  }

  if (loading) return <p className="page">Cargando…</p>

  return (
    <div className="page">
      <div className="page__head">
        <h1>{isNew ? 'Nuevo servicio' : 'Editar servicio'}</h1>
        <Link to="/services" className="btn btn--ghost">
          Volver
        </Link>
      </div>
      {!items.length && (
        <p className="banner banner--warn">
          Creá al menos un insumo antes de armar la receta.
        </p>
      )}
      {error && <p className="banner banner--warn">{error}</p>}
      <form onSubmit={(e) => void onSubmit(e)} className="form form--wide">
        <label className="field">
          <span>Nombre</span>
          <input value={name} onChange={(e) => setName(e.target.value)} required />
        </label>
        <div className="form__row">
          <label className="field">
            <span>Margen % (opcional)</span>
            <NumericTextInput
              variant="decimal"
              trimIntOnBlur={false}
              trimDecimalLeadingZerosOnBlur
              value={marginPercent}
              onChange={setMarginPercent}
              placeholder="Ej. 30"
            />
          </label>
          <label className="field">
            <span>Precio base / u. (opcional)</span>
            <NumericTextInput
              variant="decimal"
              trimIntOnBlur={false}
              trimDecimalLeadingZerosOnBlur
              value={basePrice}
              onChange={setBasePrice}
              placeholder="Piso de venta"
            />
          </label>
        </div>

        <h2 className="form__section-title">Insumos por unidad de servicio</h2>
        {lines.map((line, i) => (
          <div key={i} className="service-line">
            <select
              value={line.itemId}
              onChange={(e) => updateLine(i, { itemId: e.target.value })}
            >
              {items.map((it) => (
                <option key={it.id} value={it.id}>
                  {it.name} ({it.unit})
                </option>
              ))}
            </select>
            <label>
              <span className="sr-only">Cantidad usada</span>
              <NumericTextInput
                variant="decimal"
                trimIntOnBlur={false}
                trimDecimalLeadingZerosOnBlur
                value={line.quantityStr}
                onChange={(quantityStr) => updateLine(i, { quantityStr })}
              />
            </label>
            <button
              type="button"
              className="btn btn--small btn--danger"
              onClick={() => removeLine(i)}
            >
              Quitar
            </button>
          </div>
        ))}
        <button
          type="button"
          className="btn btn--ghost"
          onClick={addLine}
          disabled={!items.length}
        >
          + Línea de insumo
        </button>

        <div className="form__actions">
          {!isNew && (
            <button
              type="button"
              className="btn btn--danger"
              onClick={() => void onDelete()}
            >
              Eliminar servicio
            </button>
          )}
          <button type="submit" className="btn btn--primary" disabled={saving}>
            {saving ? 'Guardando…' : 'Guardar'}
          </button>
        </div>
      </form>
    </div>
  )
}
