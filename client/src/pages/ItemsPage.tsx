import { useEffect, useState } from 'react'
import { NumericTextInput } from '../components/NumericTextInput'
import * as itemsApi from '../api/items'
import { parseDecimal } from '../lib/numericStrings'
import type { CreateItemDto, ItemDto, UpdateItemDto } from '../types/api'

const emptyDraft = (): CreateItemDto => ({
  name: '',
  unit: '',
  costPerUnit: 0,
})

export function ItemsPage() {
  const [list, setList] = useState<ItemDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [draft, setDraft] = useState<CreateItemDto>(emptyDraft())
  const [costStr, setCostStr] = useState('')

  async function load() {
    setLoading(true)
    setError('')
    try {
      const data = await itemsApi.listItems()
      setList(data)
    } catch {
      setError('No se pudieron cargar los insumos.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  function openCreate() {
    setEditingId(null)
    setDraft(emptyDraft())
    setCostStr('')
    setDialogOpen(true)
  }

  function openEdit(item: ItemDto) {
    setEditingId(item.id)
    setDraft({
      name: item.name,
      unit: item.unit,
      costPerUnit: item.costPerUnit,
    })
    setCostStr(String(item.costPerUnit))
    setDialogOpen(true)
  }

  function closeDialog() {
    setDialogOpen(false)
    setEditingId(null)
    setDraft(emptyDraft())
    setCostStr('')
  }

  async function saveItem(e: React.FormEvent) {
    e.preventDefault()
    setError('')
    const cost = parseDecimal(costStr)
    if (cost === null || cost < 0) {
      setError('Indicá un costo por unidad válido (≥ 0).')
      return
    }
    const body: CreateItemDto = { ...draft, costPerUnit: cost }
    try {
      if (editingId) {
        const upd: UpdateItemDto = { ...body }
        const updated = await itemsApi.updateItem(editingId, upd)
        setList((prev) => prev.map((x) => (x.id === updated.id ? updated : x)))
      } else {
        const created = await itemsApi.createItem(body)
        setList((prev) => [...prev, created])
      }
      closeDialog()
    } catch {
      setError('Error al guardar.')
    }
  }

  async function remove(id: string) {
    if (!confirm('¿Eliminar este insumo?')) return
    try {
      await itemsApi.deleteItem(id)
      setList((prev) => prev.filter((x) => x.id !== id))
    } catch {
      setError('No se pudo eliminar.')
    }
  }

  const fmt = new Intl.NumberFormat('es-AR', {
    style: 'currency',
    currency: 'ARS',
    maximumFractionDigits: 4,
  })

  return (
    <div className="page">
      <div className="page__head">
        <h1>Insumos</h1>
        <button type="button" className="btn btn--primary" onClick={openCreate}>
          Nuevo insumo
        </button>
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
                <th>Unidad</th>
                <th>Costo / u.</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {list.map((row) => (
                <tr key={row.id}>
                  <td>{row.name}</td>
                  <td>{row.unit}</td>
                  <td>{fmt.format(row.costPerUnit)}</td>
                  <td className="table__actions">
                    <button
                      type="button"
                      className="btn btn--small btn--ghost"
                      onClick={() => openEdit(row)}
                    >
                      Editar
                    </button>
                    <button
                      type="button"
                      className="btn btn--small btn--danger"
                      onClick={() => void remove(row.id)}
                    >
                      Borrar
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {dialogOpen && (
        <dialog open className="modal" aria-labelledby="item-dialog-title">
          <form onSubmit={(e) => void saveItem(e)} className="modal__inner">
            <h2 id="item-dialog-title">
              {editingId ? 'Editar insumo' : 'Nuevo insumo'}
            </h2>
            <label className="field">
              <span>Nombre</span>
              <input
                value={draft.name}
                onChange={(e) => setDraft((d) => ({ ...d, name: e.target.value }))}
                required
              />
            </label>
            <label className="field">
              <span>Unidad</span>
              <input
                value={draft.unit}
                onChange={(e) => setDraft((d) => ({ ...d, unit: e.target.value }))}
                required
                placeholder="kg, m², h…"
              />
            </label>
            <label className="field">
              <span>Costo por unidad</span>
              <NumericTextInput
                variant="decimal"
                trimIntOnBlur={false}
                trimDecimalLeadingZerosOnBlur
                value={costStr}
                onChange={setCostStr}
                required
              />
            </label>
            <div className="modal__actions">
              <button type="button" className="btn btn--ghost" onClick={closeDialog}>
                Cancelar
              </button>
              <button type="submit" className="btn btn--primary">
                Guardar
              </button>
            </div>
          </form>
        </dialog>
      )}
    </div>
  )
}
