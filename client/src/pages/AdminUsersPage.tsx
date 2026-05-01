import axios from 'axios'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { ConfirmDialog } from '../components/admin/ConfirmDialog'
import { StatsCards } from '../components/admin/StatsCards'
import { UsersTable } from '../components/admin/UsersTable'
import { useAuth } from '../context/AuthContext'
import { getStats, getUserDetail, getUsers, toggleUser } from '../services/adminService'
import type {
  AdminStatsDto,
  AdminUserDetailDto,
  AdminUserListItemDto,
} from '../types/api'

type FilterStatus = 'all' | 'active' | 'inactive'

function formatDt(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString('es-AR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch {
    return iso
  }
}

function roleLabel(role: number) {
  if (role === 0) return 'Admin (tenant)'
  if (role === 1) return 'Usuario'
  return `Rol ${role}`
}

export function AdminUsersPage() {
  const { userId } = useAuth()

  // ── Datos principales ──
  const [stats, setStats] = useState<AdminStatsDto | null>(null)
  const [users, setUsers] = useState<AdminUserListItemDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<string | null>(null)

  // ── Modal detalle ──
  const [detail, setDetail] = useState<AdminUserDetailDto | null>(null)
  const [detailOpen, setDetailOpen] = useState(false)
  const [detailLoading, setDetailLoading] = useState(false)

  // ── Confirmación toggle ──
  const [pendingToggle, setPendingToggle] = useState<AdminUserListItemDto | null>(null)

  // ── Filtros / búsqueda ──
  const [search, setSearch] = useState('')
  const [filterStatus, setFilterStatus] = useState<FilterStatus>('all')

  // ── Carga de datos ──
  const load = useCallback(async (signal?: AbortSignal) => {
    setError(null)
    setLoading(true)
    try {
      const [s, u] = await Promise.all([getStats(signal), getUsers(signal)])
      setStats(s)
      setUsers(u)
    } catch (e: unknown) {
      if (axios.isAxiosError(e) && e.code === 'ERR_CANCELED') return
      const msg =
        e && typeof e === 'object' && 'message' in e
          ? String((e as { message: unknown }).message)
          : 'No se pudo cargar el panel.'
      setError(msg)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    const ac = new AbortController()
    void load(ac.signal)
    return () => ac.abort()
  }, [load])

  // ── Usuarios filtrados ──
  const filteredUsers = useMemo(() => {
    const q = search.trim().toLowerCase()
    return users.filter((u) => {
      const matchSearch =
        q === '' ||
        u.email.toLowerCase().includes(q) ||
        (u.tenantName?.toLowerCase().includes(q) ?? false)
      const matchStatus =
        filterStatus === 'all' ||
        (filterStatus === 'active' && u.isActive) ||
        (filterStatus === 'inactive' && !u.isActive)
      return matchSearch && matchStatus
    })
  }, [users, search, filterStatus])

  // ── Detalle ──
  const openDetail = async (id: string) => {
    setDetailOpen(true)
    setDetail(null)
    setDetailLoading(true)
    try {
      const d = await getUserDetail(id)
      setDetail(d)
    } catch {
      window.alert('No se pudo cargar el detalle del usuario.')
      setDetailOpen(false)
    } finally {
      setDetailLoading(false)
    }
  }

  const closeDetail = () => {
    setDetailOpen(false)
    setDetail(null)
  }

  // ── Toggle activo/inactivo ──
  const requestToggle = (user: AdminUserListItemDto) => {
    if (userId && user.id === userId) {
      window.alert('No podés desactivar tu propia cuenta desde aquí.')
      return
    }
    setPendingToggle(user)
  }

  const confirmToggle = async () => {
    if (!pendingToggle) return
    const { id } = pendingToggle
    setPendingToggle(null)
    setBusyId(id)
    setError(null)
    try {
      await toggleUser(id)
      await load()
      if (detail?.id === id) {
        const d = await getUserDetail(id)
        setDetail(d)
      }
    } catch {
      setError('No se pudo actualizar el usuario.')
    } finally {
      setBusyId(null)
    }
  }

  const cancelToggle = () => setPendingToggle(null)

  return (
    <div className="page page--admin">
      {/* ── Encabezado ── */}
      <div className="page__head">
        <div>
          <h1>Administración de usuarios</h1>
          <p className="lead" style={{ marginBottom: 0 }}>
            Usuarios registrados, acceso a la app y fechas de caducidad.
          </p>
        </div>
        <button
          type="button"
          id="admin-refresh-btn"
          className="btn btn--ghost"
          onClick={() => void load()}
          disabled={loading}
        >
          {loading ? 'Cargando…' : 'Actualizar'}
        </button>
      </div>

      {/* ── Error global ── */}
      {error ? (
        <div className="banner banner--warn" role="alert">
          {error}
        </div>
      ) : null}

      {/* ── Stats ── */}
      {stats ? <StatsCards stats={stats} /> : null}

      {/* ── Buscador y filtros ── */}
      <div className="admin-filters" style={{ marginTop: '1.5rem' }}>
        <input
          id="admin-search"
          type="search"
          placeholder="Buscar por email u organización…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="admin-filters__search"
          aria-label="Buscar usuario por email u organización"
        />
        <div className="admin-filters__tabs" role="group" aria-label="Filtrar por estado">
          {(['all', 'active', 'inactive'] as FilterStatus[]).map((f) => (
            <button
              key={f}
              type="button"
              id={`admin-filter-${f}`}
              className={`btn btn--small admin-filters__tab${filterStatus === f ? ' admin-filters__tab--active' : ''}`}
              onClick={() => setFilterStatus(f)}
            >
              {f === 'all' ? 'Todos' : f === 'active' ? 'Activos' : 'Inactivos'}
            </button>
          ))}
        </div>
      </div>

      {/* ── Tabla ── */}
      <section style={{ marginTop: '1rem' }}>
        <h2 className="sr-only">Listado de usuarios</h2>
        {loading ? (
          <p className="lead">Cargando…</p>
        ) : (
          <UsersTable
            users={filteredUsers}
            busyId={busyId}
            currentUserId={userId}
            onToggle={requestToggle}
            onDetail={(id) => void openDetail(id)}
          />
        )}
      </section>

      {/* ── Modal: confirmación toggle ── */}
      {pendingToggle ? (
        <ConfirmDialog
          title={pendingToggle.isActive ? 'Desactivar usuario' : 'Activar usuario'}
          message={
            pendingToggle.isActive
              ? `¿Seguro que querés desactivar a ${pendingToggle.email}? No podrá ingresar a la app.`
              : `¿Querés activar a ${pendingToggle.email}? Recuperará el acceso a la app.`
          }
          confirmLabel={pendingToggle.isActive ? 'Sí, desactivar' : 'Sí, activar'}
          cancelLabel="Cancelar"
          danger={pendingToggle.isActive}
          onConfirm={() => void confirmToggle()}
          onCancel={cancelToggle}
        />
      ) : null}

      {/* ── Modal: detalle de usuario ── */}
      {detailOpen ? (
        <div
          className="modal-backdrop"
          role="presentation"
          onClick={(ev) => {
            if (ev.target === ev.currentTarget) closeDetail()
          }}
        >
          <div className="modal" role="dialog" aria-modal="true" aria-labelledby="admin-detail-title">
            <div className="modal__head">
              <h2 id="admin-detail-title">Detalle de usuario</h2>
              <button
                type="button"
                id="admin-detail-close"
                className="btn btn--ghost btn--small"
                onClick={closeDetail}
              >
                Cerrar
              </button>
            </div>
            <div className="modal__body">
              {detailLoading ? (
                <p className="lead">Cargando…</p>
              ) : detail ? (
                <dl className="detail-list">
                  <div>
                    <dt>Email</dt>
                    <dd>{detail.email}</dd>
                  </div>
                  <div>
                    <dt>Estado</dt>
                    <dd>
                      {detail.isActive ? (
                        <span className="badge badge--ok">Activo</span>
                      ) : (
                        <span className="badge badge--off">Inactivo</span>
                      )}
                    </dd>
                  </div>
                  <div>
                    <dt>Rol</dt>
                    <dd>{roleLabel(detail.role)}</dd>
                  </div>
                  <div>
                    <dt>Super admin</dt>
                    <dd>{detail.isSuperAdmin ? 'Sí' : 'No'}</dd>
                  </div>
                  <div>
                    <dt>Tenant</dt>
                    <dd>
                      {detail.tenantName ?? '—'}
                      {detail.tenantId ? (
                        <code className="detail-list__mono"> {detail.tenantId.slice(0, 8)}…</code>
                      ) : null}
                    </dd>
                  </div>
                  <div>
                    <dt>Plan</dt>
                    <dd>
                      {detail.planName ? (
                        <span className="badge" style={{ background: 'var(--surface2)', color: 'var(--text)' }}>
                          {detail.planName}
                        </span>
                      ) : (
                        <span style={{ color: 'var(--muted)', fontSize: '0.85rem' }}>—</span>
                      )}
                    </dd>
                  </div>
                  <div>
                    <dt>Vencimiento plan</dt>
                    <dd>{formatDt(detail.planEndDate)}</dd>
                  </div>
                  <div>
                    <dt>Caducidad cuenta</dt>

                    <dd>{formatDt(detail.expirationDate)}</dd>
                  </div>
                  <div>
                    <dt>Alta</dt>
                    <dd>{formatDt(detail.createdAt)}</dd>
                  </div>
                  <div>
                    <dt>Último acceso</dt>
                    <dd>{formatDt(detail.lastLogin)}</dd>
                  </div>
                  <div>
                    <dt>Dispositivos</dt>
                    <dd>{detail.deviceCount}</dd>
                  </div>
                </dl>
              ) : (
                <p className="lead">Sin datos.</p>
              )}
            </div>
          </div>
        </div>
      ) : null}
    </div>
  )
}
