export interface RemoteAppStatusDto {
  id: string
  appEnabled: boolean
  maintenanceMode: boolean
  forceUpdate: boolean
  blocked: boolean
  clientVersion: string | null
  minimumVersion: string | null
  blockedVersions: string[]
  message: string
  status: string
}

export interface TokenResponseDto {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAt: string
  refreshTokenExpiresAt: string
  userId: string
  /** Null para sesión de super administrador (sin tenant). */
  tenantId: string | null
  /** Nombre de la organización (tenant); null si super admin o no enviado. */
  tenantName: string | null
}

/** Panel admin (API /api/admin/...). */
export interface AdminStatsDto {
  totalUsers: number
  activeUsers: number
  inactiveUsers: number
}

export interface AdminUserListItemDto {
  id: string
  email: string
  isActive: boolean
  createdAt: string
  lastLogin: string | null
  tenantId: string | null
  tenantName: string | null
}

export interface AdminUserDetailDto {
  id: string
  email: string
  isActive: boolean
  isSuperAdmin: boolean
  /** 0 = Admin del tenant, 1 = Usuario (según enum backend). */
  role: number
  createdAt: string
  lastLogin: string | null
  expirationDate: string | null
  tenantId: string | null
  tenantName: string | null
  deviceCount: number
}

export interface AdminToggleActiveResponseDto {
  isActive: boolean
}

export interface LoginRequestDto {
  email: string
  password: string
  deviceId?: string
  deviceName?: string
}

export interface RegisterRequestDto extends LoginRequestDto {
  tenantName: string
}

export interface ItemDto {
  id: string
  name: string
  unit: string
  costPerUnit: number
}

export interface CreateItemDto {
  name: string
  unit: string
  costPerUnit: number
}

export interface UpdateItemDto extends CreateItemDto {}

export interface ServiceItemLineDto {
  itemId: string
  quantityUsed: number
}

export interface ServiceItemLineResponseDto {
  id: string
  itemId: string
  itemName: string
  quantityUsed: number
}

export interface ServiceDto {
  id: string
  name: string
  basePrice: number | null
  marginPercent: number | null
  serviceItems: ServiceItemLineResponseDto[]
}

export interface CreateServiceDto {
  name: string
  basePrice: number | null
  marginPercent: number | null
  serviceItems: ServiceItemLineDto[]
}

export interface UpdateServiceDto extends CreateServiceDto {}

export interface BudgetLineInputDto {
  serviceId: string
  quantity: number
  manualPriceOverride: number | null
}

export interface CreateBudgetDto {
  lines: BudgetLineInputDto[]
}

export interface BudgetDetailDto {
  id: string
  serviceId: string
  serviceName: string
  quantity: number
  calculatedCost: number
  calculatedPrice: number
  manualPriceOverride: number | null
}

export interface BudgetDto {
  id: string
  totalCost: number
  totalPrice: number
  createdAt: string
  createdByUserId: string | null
  details: BudgetDetailDto[]
}
