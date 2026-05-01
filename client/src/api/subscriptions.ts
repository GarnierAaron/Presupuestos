import { api } from './client'
import type {
  CreateSubscriptionRequestDto,
  CreateSubscriptionResponseDto,
  MySubscriptionResponseDto,
} from '../types/api'

export async function createSubscription(dto: CreateSubscriptionRequestDto, signal?: AbortSignal) {
  const { data } = await api.post<CreateSubscriptionResponseDto>('/api/subscriptions/create', dto, {
    signal,
  })
  return data
}

export async function fetchMySubscription(signal?: AbortSignal) {
  const { data } = await api.get<MySubscriptionResponseDto>('/api/subscriptions/me', { signal })
  return data
}
