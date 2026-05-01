import { api } from './client'
import type { CreateItemDto, ItemDto, UpdateItemDto } from '../types/api'

export async function listItems(signal?: AbortSignal) {
  const { data } = await api.get<ItemDto[]>('/api/Items', { signal })
  return data
}

export async function getItem(id: string) {
  const { data } = await api.get<ItemDto>(`/api/Items/${id}`)
  return data
}

export async function createItem(body: CreateItemDto) {
  const { data } = await api.post<ItemDto>('/api/Items', body)
  return data
}

export async function updateItem(id: string, body: UpdateItemDto) {
  const { data } = await api.put<ItemDto>(`/api/Items/${id}`, body)
  return data
}

export async function deleteItem(id: string) {
  await api.delete(`/api/Items/${id}`)
}
