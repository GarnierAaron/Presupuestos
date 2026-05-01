import { api } from './client'
import type { BudgetDto, CreateBudgetDto } from '../types/api'

export async function listBudgets() {
  const { data } = await api.get<BudgetDto[]>('/api/Budgets')
  return data
}

export async function getBudget(id: string) {
  const { data } = await api.get<BudgetDto>(`/api/Budgets/${id}`)
  return data
}

export async function createBudget(body: CreateBudgetDto) {
  const { data } = await api.post<BudgetDto>('/api/Budgets', body)
  return data
}

export async function deleteBudget(id: string) {
  await api.delete(`/api/Budgets/${id}`)
}
