/**
 * adminService.ts
 * Capa de servicio para el panel de administración.
 * Encapsula las llamadas a la API admin y expone funciones tipadas.
 */
import {
  fetchAdminStats,
  fetchAdminUser,
  fetchAdminUsers,
  toggleAdminUserActive,
} from '../api/admin'
import type {
  AdminStatsDto,
  AdminUserDetailDto,
  AdminUserListItemDto,
} from '../types/api'

/** Obtiene las estadísticas globales del sistema. */
export async function getStats(signal?: AbortSignal): Promise<AdminStatsDto> {
  return fetchAdminStats(signal)
}

/** Obtiene la lista de todos los usuarios. */
export async function getUsers(signal?: AbortSignal): Promise<AdminUserListItemDto[]> {
  return fetchAdminUsers(signal)
}

/** Obtiene el detalle completo de un usuario. */
export async function getUserDetail(id: string, signal?: AbortSignal): Promise<AdminUserDetailDto> {
  return fetchAdminUser(id, signal)
}

/** Alterna el estado activo/inactivo de un usuario. */
export async function toggleUser(id: string): Promise<{ isActive: boolean }> {
  return toggleAdminUserActive(id)
}
