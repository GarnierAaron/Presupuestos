import type { AdminStatsDto } from '../../types/api'

interface StatsCardsProps {
  stats: AdminStatsDto
}

export function StatsCards({ stats }: StatsCardsProps) {
  return (
    <div className="admin-stat-grid">
      <div className="admin-stat">
        <span className="admin-stat__label">Total usuarios</span>
        <strong className="admin-stat__value">{stats.totalUsers}</strong>
      </div>
      <div className="admin-stat admin-stat--ok">
        <span className="admin-stat__label">Activos</span>
        <strong className="admin-stat__value admin-stat__value--ok">{stats.activeUsers}</strong>
      </div>
      <div className="admin-stat admin-stat--off">
        <span className="admin-stat__label">Inactivos</span>
        <strong className="admin-stat__value admin-stat__value--off">{stats.inactiveUsers}</strong>
      </div>
    </div>
  )
}
