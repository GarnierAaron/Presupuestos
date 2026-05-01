import { plainApi } from './client'
import type { RemoteAppStatusDto } from '../types/api'

export async function fetchAppConfig(): Promise<RemoteAppStatusDto> {
  const version = import.meta.env.VITE_APP_VERSION ?? '0.0.0'
  const { data } = await plainApi.get<RemoteAppStatusDto>('/api/app-config', {
    params: { version },
    headers: { 'X-App-Version': version },
  })
  return data
}
