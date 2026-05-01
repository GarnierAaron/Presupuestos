import type { AdminUserListItemDto } from '../../types/api'
import { UserRow } from './UserRow'

interface UsersTableProps {
  users: AdminUserListItemDto[]
  busyId: string | null
  currentUserId: string | null
  onToggle: (user: AdminUserListItemDto) => void
  onDetail: (id: string) => void
}

export function UsersTable({
  users,
  busyId,
  currentUserId,
  onToggle,
  onDetail,
}: UsersTableProps) {
  if (users.length === 0) {
    return <p className="lead" style={{ marginTop: '2rem', textAlign: 'center' }}>No hay usuarios que coincidan.</p>
  }

  return (
    <div className="table-wrap">
      <table className="table">
        <thead>
          <tr>
            <th>Email</th>
            <th>Organización</th>
            <th>Plan</th>
            <th>Fin de plan</th>
            <th>Estado</th>
            <th>Fecha de alta</th>
            <th>Último login</th>
            <th style={{ width: '1%' }}>Acciones</th>
          </tr>
        </thead>
        <tbody>
          {users.map((u) => (
            <UserRow
              key={u.id}
              user={u}
              isBusy={busyId === u.id}
              isCurrentUser={currentUserId !== null && u.id === currentUserId}
              onToggle={() => onToggle(u)}
              onDetail={() => onDetail(u.id)}
            />
          ))}
        </tbody>
      </table>
    </div>
  )
}
