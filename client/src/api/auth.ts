import { plainApi } from './client'
import type { LoginRequestDto, RegisterRequestDto, TokenResponseDto } from '../types/api'

export async function login(body: LoginRequestDto) {
  const { data } = await plainApi.post<TokenResponseDto>('/api/Auth/login', body)
  return data
}

export async function register(body: RegisterRequestDto) {
  const { data } = await plainApi.post<TokenResponseDto>('/api/Auth/register', body)
  return data
}

export async function logout(refreshToken: string) {
  await plainApi.post('/api/Auth/logout', { refreshToken })
}
