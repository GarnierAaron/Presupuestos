import { api } from './client'
import type {
  AdminStatsDto,
  AdminToggleActiveResponseDto,
  AdminUserDetailDto,
  AdminUserListItemDto,
} from '../types/api'

export async function fetchAdminStats(signal?: AbortSignal) {
  const { data } = await api.get<AdminStatsDto>('/api/admin/stats', { signal })
  return data
}

export async function fetchAdminUsers(signal?: AbortSignal) {
  const { data } = await api.get<AdminUserListItemDto[]>('/api/admin/users', { signal })
  return data
}

export async function fetchAdminUser(id: string, signal?: AbortSignal) {
  const { data } = await api.get<AdminUserDetailDto>(`/api/admin/users/${id}`, { signal })
  return data
}

export async function toggleAdminUserActive(id: string) {
  const { data } = await api.patch<AdminToggleActiveResponseDto>(
    `/api/admin/users/${id}/toggle-active`
  )
  return data
}
