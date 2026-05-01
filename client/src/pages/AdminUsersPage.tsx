import axios from 'axios'
import { useCallback, useEffect, useState } from 'react'
import {
  fetchAdminStats,
  fetchAdminUser,
  fetchAdminUsers,
  toggleAdminUserActive,
} from '../api/admin'
import { useAuth } from '../context/AuthContext'
import type { AdminStatsDto, AdminUserDetailDto, AdminUserListItemDto } from '../types/api'

function formatDt(iso: string | null) {
  if (!iso) return '—'
  try {
    return new Date(iso).toLocaleString()
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
  const [stats, setStats] = useState<AdminStatsDto | null>(null)
  const [users, setUsers] = useState<AdminUserListItemDto[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [busyId, setBusyId] = useState<string | null>(null)

  const [detail, setDetail] = useState<AdminUserDetailDto | null>(null)
  const [detailOpen, setDetailOpen] = useState(false)
  const [detailLoading, setDetailLoading] = useState(false)

  const load = useCallback(async (signal?: AbortSignal) => {
    setError(null)
    setLoading(true)
    try {
      const [s, u] = await Promise.all([
        fetchAdminStats(signal),
        fetchAdminUsers(signal),
      ])
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

  const openDetail = async (id: string) => {
    setDetailOpen(true)
    setDetail(null)
    setDetailLoading(true)
    try {
      const d = await fetchAdminUser(id)
      setDetail(d)
    } catch {
      setDetail(null)
      window.alert('No se pudo cargar el detalle del usuario.')
    } finally {
      setDetailLoading(false)
    }
  }

  const closeDetail = () => {
    setDetailOpen(false)
    setDetail(null)
  }

  const onToggle = async (id: string, email: string) => {
    if (userId && id === userId) {
      window.alert('No podés desactivar tu propia cuenta desde aquí.')
      return
    }
    const ok = window.confirm(
      `¿Cambiar el estado activo/inactivo de ${email}?\nLos usuarios inactivos no podrán usar la app (403).`
    )
    if (!ok) return
    setBusyId(id)
    setError(null)
    try {
      await toggleAdminUserActive(id)
      await load()
      if (detail?.id === id) {
        const d = await fetchAdminUser(id)
        setDetail(d)
      }
    } catch {
      setError('No se pudo actualizar el usuario.')
    } finally {
      setBusyId(null)
    }
  }

  return (
    <div className="page page--admin">
      <div className="page__head">
        <div>
          <h1>Administración</h1>
          <p className="lead" style={{ marginBottom: 0 }}>
            Usuarios registrados, acceso a la app y vista previa hacia suscripciones (fechas de
            caducidad).
          </p>
        </div>
        <button type="button" className="btn btn--ghost" onClick={() => void load()} disabled={loading}>
          Actualizar
        </button>
      </div>

      {error ? (
        <div className="banner banner--warn" role="alert">
          {error}
        </div>
      ) : null}

      {stats ? (
        <div className="admin-stat-grid">
          <div className="admin-stat">
            <span className="admin-stat__label">Total</span>
            <strong className="admin-stat__value">{stats.totalUsers}</strong>
          </div>
          <div className="admin-stat">
            <span className="admin-stat__label">Activos</span>
            <strong className="admin-stat__value admin-stat__value--ok">{stats.activeUsers}</strong>
          </div>
          <div className="admin-stat">
            <span className="admin-stat__label">Inactivos</span>
            <strong className="admin-stat__value admin-stat__value--off">
              {stats.inactiveUsers}
            </strong>
          </div>
        </div>
      ) : null}

      <section style={{ marginTop: '1.5rem' }}>
        <h2 className="sr-only">Listado</h2>
        {loading ? (
          <p className="lead">Cargando…</p>
        ) : (
          <div className="table-wrap">
            <table className="table">
              <thead>
                <tr>
                  <th>Email</th>
                  <th>Estado</th>
                  <th>Alta</th>
                  <th>Último acceso</th>
                  <th style={{ width: '1%' }}>Acciones</th>
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id}>
                    <td>{u.email}</td>
                    <td>
                      {u.isActive ? (
                        <span className="badge badge--ok">Activo</span>
                      ) : (
                        <span className="badge badge--off">Inactivo</span>
                      )}
                    </td>
                    <td>{formatDt(u.createdAt)}</td>
                    <td>{formatDt(u.lastLogin)}</td>
                    <td>
                      <div className="table__actions">
                        <button
                          type="button"
                          className="btn btn--ghost btn--small"
                          onClick={() => void openDetail(u.id)}
                        >
                          Detalle
                        </button>
                        <button
                          type="button"
                          className="btn btn--small"
                          style={{
                            background: 'var(--surface2)',
                            color: 'var(--text)',
                          }}
                          disabled={busyId === u.id || (userId !== null && u.id === userId)}
                          onClick={() => void onToggle(u.id, u.email)}
                        >
                          {u.isActive ? 'Desactivar' : 'Activar'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

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
              <button type="button" className="btn btn--ghost btn--small" onClick={closeDetail}>
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
                    <dt>Dispositivos registrados</dt>
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
