import { useCallback, useEffect, useState } from 'react'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const SIDEBAR_BREAKPOINT_PX = 900

export function Layout() {
  const { logout, tenantId, tenantName, isSuperAdmin } = useAuth()
  const location = useLocation()
  const [sidebarOpen, setSidebarOpen] = useState(false)

  const closeSidebar = useCallback(() => setSidebarOpen(false), [])

  useEffect(() => {
    closeSidebar()
  }, [location.pathname, closeSidebar])

  useEffect(() => {
    if (!sidebarOpen) return
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') closeSidebar()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [sidebarOpen, closeSidebar])

  useEffect(() => {
    if (!sidebarOpen) return
    const mq = window.matchMedia(`(max-width: ${SIDEBAR_BREAKPOINT_PX - 1}px)`)
    const apply = () => {
      document.body.style.overflow = mq.matches ? 'hidden' : ''
    }
    apply()
    mq.addEventListener('change', apply)
    return () => {
      mq.removeEventListener('change', apply)
      document.body.style.overflow = ''
    }
  }, [sidebarOpen])

  const navClass = ({ isActive }: { isActive: boolean }) =>
    'nav-link nav-link--sidebar' + (isActive ? ' active' : '')

  return (
    <div className={`layout${sidebarOpen ? ' layout--nav-open' : ''}`}>
      <button
        type="button"
        className="layout__sidebar-scrim"
        aria-label="Cerrar menú"
        tabIndex={sidebarOpen ? 0 : -1}
        onClick={closeSidebar}
      />

      <aside id="app-sidebar" className="layout__sidebar" aria-label="Navegación principal">
        <div className="layout__sidebar-brand">
          <span className="layout__logo" aria-hidden />
          <strong>Presupuestos</strong>
        </div>

        <nav className="layout__sidebar-nav">
          <NavLink to="/" end className={navClass} onClick={closeSidebar}>
            Inicio
          </NavLink>
          {isSuperAdmin ? (
            <NavLink
              to="/admin"
              className={({ isActive }) =>
                'nav-link nav-link--sidebar nav-link--admin' + (isActive ? ' active' : '')
              }
              onClick={closeSidebar}
            >
              Administración
            </NavLink>
          ) : (
            <>
              <NavLink to="/items" className={navClass} onClick={closeSidebar}>
                Insumos
              </NavLink>
              <NavLink to="/services" className={navClass} onClick={closeSidebar}>
                Servicios
              </NavLink>
              <NavLink to="/budgets" className={navClass} onClick={closeSidebar}>
                Presupuestos
              </NavLink>
              <NavLink to="/budgets/new" className={navClass} onClick={closeSidebar}>
                Nuevo presupuesto
              </NavLink>
              <NavLink to="/pricing" className={navClass} onClick={closeSidebar}>
                Suscripción
              </NavLink>
            </>
          )}
        </nav>

        <div className="layout__sidebar-footer">
          <span
            className="layout__tenant layout__tenant--name"
            title={
              isSuperAdmin
                ? 'Super administrador'
                : [tenantName, tenantId].filter(Boolean).join(' · ') || ''
            }
          >
            {isSuperAdmin
              ? 'Super admin'
              : tenantName?.trim() ||
                (tenantId ? `${tenantId.slice(0, 8)}…` : '—')}
          </span>
          <button type="button" className="btn btn--ghost btn--small" onClick={() => void logout()}>
            Salir
          </button>
        </div>
      </aside>

      <div className="layout__shell">
        <header className="layout__topbar">
          <button
            type="button"
            className="layout__menu-btn"
            aria-controls="app-sidebar"
            aria-expanded={sidebarOpen}
            aria-label={sidebarOpen ? 'Cerrar menú de navegación' : 'Abrir menú de navegación'}
            onClick={() => setSidebarOpen((o) => !o)}
          >
            <span className="layout__menu-bars" aria-hidden>
              <span />
              <span />
              <span />
            </span>
          </button>
          <span className="layout__topbar-title">Presupuestos</span>
        </header>

        <main className="layout__main">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
