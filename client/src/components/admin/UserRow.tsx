import type { AdminUserListItemDto } from '../../types/api'

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

interface UserRowProps {
  user: AdminUserListItemDto
  isBusy: boolean
  isCurrentUser: boolean
  onToggle: () => void
  onDetail: () => void
}

export function UserRow({ user, isBusy, isCurrentUser, onToggle, onDetail }: UserRowProps) {
  return (
    <tr>
      <td style={{ maxWidth: 220, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
        {user.email}
      </td>
      <td style={{ maxWidth: 180, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
        {user.tenantName ? (
          <span>{user.tenantName}</span>
        ) : (
          <span style={{ color: 'var(--muted)', fontSize: '0.85rem' }}>—</span>
        )}
      </td>
      <td>
        {user.planName ? (
          <span className="badge" style={{ background: 'var(--surface2)', color: 'var(--text)' }}>
            {user.planName}
          </span>
        ) : (
          <span style={{ color: 'var(--muted)', fontSize: '0.85rem' }}>—</span>
        )}
      </td>
      <td>{formatDt(user.planEndDate)}</td>
      <td>
        {user.isActive ? (
          <span className="badge badge--ok">Activo</span>
        ) : (
          <span className="badge badge--off">Inactivo</span>
        )}
      </td>
      <td>{formatDt(user.createdAt)}</td>
      <td>{formatDt(user.lastLogin)}</td>
      <td>
        <div className="table__actions">
          <button
            type="button"
            id={`admin-detail-btn-${user.id}`}
            className="btn btn--ghost btn--small"
            onClick={onDetail}
          >
            Detalle
          </button>
          <button
            type="button"
            id={`admin-toggle-btn-${user.id}`}
            className={`btn btn--small ${user.isActive ? 'btn--toggle-off' : 'btn--toggle-on'}`}
            disabled={isBusy || isCurrentUser}
            title={isCurrentUser ? 'No podés desactivar tu propia cuenta' : undefined}
            onClick={onToggle}
          >
            {isBusy ? '…' : user.isActive ? 'Desactivar' : 'Activar'}
          </button>
        </div>
      </td>
    </tr>
  )
}
