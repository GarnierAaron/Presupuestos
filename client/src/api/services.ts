import { api } from './client'
import type { CreateServiceDto, ServiceDto, UpdateServiceDto } from '../types/api'

export async function listServices(signal?: AbortSignal) {
  const { data } = await api.get<ServiceDto[]>('/api/Services', { signal })
  return data
}

export async function getService(id: string) {
  const { data } = await api.get<ServiceDto>(`/api/Services/${id}`)
  return data
}

export async function createService(body: CreateServiceDto) {
  const { data } = await api.post<ServiceDto>('/api/Services', body)
  return data
}

export async function updateService(id: string, body: UpdateServiceDto) {
  const { data } = await api.put<ServiceDto>(`/api/Services/${id}`, body)
  return data
}

export async function deleteService(id: string) {
  await api.delete(`/api/Services/${id}`)
}
