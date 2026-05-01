import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { KillSwitchGate } from './components/KillSwitchGate'
import { Layout } from './components/Layout'
import { ProtectedRoute } from './components/ProtectedRoute'
import { SuperAdminRoute } from './components/SuperAdminRoute'
import { AuthProvider } from './context/AuthContext'
import { BudgetCreatePage } from './pages/BudgetCreatePage'
import { BudgetsListPage } from './pages/BudgetsListPage'
import { AdminUsersPage } from './pages/AdminUsersPage'
import { DashboardPage } from './pages/DashboardPage'
import { ItemsPage } from './pages/ItemsPage'
import { LoginPage } from './pages/LoginPage'
import { PricingPage } from './pages/PricingPage'
import { ServiceEditPage } from './pages/ServiceEditPage'
import { ServicesPage } from './pages/ServicesPage'

export default function App() {
  return (
    <BrowserRouter>
      <KillSwitchGate>
        <AuthProvider>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route element={<ProtectedRoute />}>
              <Route element={<Layout />}>
                <Route path="/" element={<DashboardPage />} />
                <Route path="/items" element={<ItemsPage />} />
                <Route path="/services" element={<ServicesPage />} />
                <Route path="/services/:id" element={<ServiceEditPage />} />
                <Route path="/budgets" element={<BudgetsListPage />} />
                <Route path="/budgets/new" element={<BudgetCreatePage />} />
                <Route path="/pricing" element={<PricingPage />} />
                <Route element={<SuperAdminRoute />}>
                  <Route path="/admin" element={<AdminUsersPage />} />
                </Route>
              </Route>
            </Route>
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </AuthProvider>
      </KillSwitchGate>
    </BrowserRouter>
  )
}
