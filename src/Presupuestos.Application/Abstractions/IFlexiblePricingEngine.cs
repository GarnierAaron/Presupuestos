using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

/// <summary>
/// Motor de precios por reglas en cadena (porcentaje, fijo, fórmula).
/// </summary>
public interface IFlexiblePricingEngine
{
    /// <summary>
    /// Calcula el precio de venta unitario tras aplicar todas las reglas y el piso opcional <paramref name="basePriceFloor"/> (como BasePrice del servicio).
    /// </summary>
    /// <param name="unitMaterialCost">Costo unitario de insumos del servicio.</param>
    /// <param name="lineQuantity">Cantidad de la línea de presupuesto.</param>
    /// <param name="rules">Reglas del tenant ordenadas por SortOrder.</param>
    /// <param name="basePriceFloor">Piso de precio unitario del catálogo, si existe.</param>
    decimal ComputeUnitSellingPrice(
        decimal unitMaterialCost,
        decimal lineQuantity,
        IReadOnlyList<PricingRule> rules,
        decimal? basePriceFloor);
}
