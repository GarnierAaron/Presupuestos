import type { ItemDto, ServiceDto } from '../types/api'

/**
 * Réplica del flujo clásico (margen + BasePrice) para vista previa en el cliente.
 * No incluye margen global de usuario ni reglas de precio flexible del servidor.
 */
export function buildItemCostMap(items: ItemDto[]) {
  return new Map(items.map((i) => [i.id, i]))
}

export function computeUnitMaterialCost(
  service: ServiceDto,
  itemCost: Map<string, ItemDto>
): number {
  if (!service.serviceItems?.length) return 0
  let sum = 0
  for (const line of service.serviceItems) {
    const item = itemCost.get(line.itemId)
    const cpu = item?.costPerUnit ?? 0
    sum += line.quantityUsed * cpu
  }
  return sum
}

export function previewLine(
  service: ServiceDto,
  itemCost: Map<string, ItemDto>,
  quantity: number,
  manualLineTotal: number | null
): { unitCost: number; lineCost: number; linePrice: number } {
  const unitCost = computeUnitMaterialCost(service, itemCost)
  const lineCost = unitCost * quantity

  if (manualLineTotal != null) {
    return { unitCost, lineCost, linePrice: manualLineTotal }
  }

  const margin = service.marginPercent ?? 0
  const unitFromMargin = unitCost * (1 + margin / 100)
  const base = service.basePrice
  const unit =
    base == null ? unitFromMargin : Math.max(unitFromMargin, base)
  return { unitCost, lineCost, linePrice: unit * quantity }
}

export function sumPreviewLines(
  parts: { lineCost: number; linePrice: number }[]
) {
  return parts.reduce(
    (acc, p) => ({
      totalCost: acc.totalCost + p.lineCost,
      totalPrice: acc.totalPrice + p.linePrice,
    }),
    { totalCost: 0, totalPrice: 0 }
  )
}
