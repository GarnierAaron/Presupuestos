import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const cards = [
  {
    to: '/items',
    title: 'Insumos',
    desc: 'Materiales y costos por unidad.',
  },
  {
    to: '/services',
    title: 'Servicios',
    desc: 'Recetas de insumos, margen y precio base.',
  },
  {
    to: '/budgets/new',
    title: 'Nuevo presupuesto',
    desc: 'Líneas por servicio con vista previa al instante.',
  },
  {
    to: '/budgets',
    title: 'Historial',
    desc: 'Presupuestos guardados.',
  },
]

export function DashboardPage() {
  const { isSuperAdmin } = useAuth()

  if (isSuperAdmin) {
    return (
      <div className="page">
        <h1>Panel global</h1>
        <p className="lead">
          Tu sesión es de <strong>super administrador</strong>: no tenés un tenant asociado. Desde
          aquí podés controlar quién puede usar la aplicación y revisar datos básicos de cada
          cuenta.
        </p>
        <div className="card-grid" style={{ maxWidth: '480px' }}>
          <Link to="/admin" className="tile">
            <h2>Administración</h2>
            <p>Usuarios, activación / desactivación y estadísticas. Base para suscripciones mensuales.</p>
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div className="page">
      <h1>Panel</h1>
      <p className="lead">
        Gestiona insumos y servicios; arma presupuestos con totales estimados en el acto.
      </p>
      <div className="card-grid">
        {cards.map((c) => (
          <Link key={c.to} to={c.to} className="tile">
            <h2>{c.title}</h2>
            <p>{c.desc}</p>
          </Link>
        ))}
      </div>
    </div>
  )
}
