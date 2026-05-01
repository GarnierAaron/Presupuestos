using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

/// <summary>
/// Lógica de negocio pura para costo y precio. Extensible a fórmulas por rubro (Strategy) más adelante.
/// <para>
/// Ejemplo: servicio "Impresión A4" con 0.02 L tinta a 50/L = 1.00 de insumo + papel 0.15 ⇒ unitCost = 1.15.
/// Cantidad 100. Margen servicio 30% ⇒ precio/u = 1.15 × 1.30 = 1.495; si BasePrice = 2 ⇒ precio/u = max(1.495, 2) = 2.
/// Línea costo = 1.15 × 100 = 115; precio = 2 × 100 = 200 (salvo ManualPriceOverride en la línea).
/// </para>
/// </summary>
public static class BudgetCalculator
{
    public static decimal ComputeUnitCost(Service service)
    {
        if (service.ServiceItems == null || service.ServiceItems.Count == 0)
            return 0;

        decimal sum = 0;
        foreach (var si in service.ServiceItems)
        {
            var cpu = si.Item?.CostPerUnit ?? 0;
            sum += si.QuantityUsed * cpu;
        }

        return sum;
    }

    public static decimal GetEffectiveMarginPercent(Service service, decimal? userGlobalMarginPercent)
    {
        return service.MarginPercent ?? userGlobalMarginPercent ?? 0m;
    }

    public static decimal UnitPriceFromMargin(decimal unitCost, decimal marginPercent)
    {
        return unitCost * (1m + marginPercent / 100m);
    }

    public static decimal ApplyBasePriceFloor(decimal unitPriceFromMargin, decimal? basePrice)
    {
        if (!basePrice.HasValue) return unitPriceFromMargin;
        return Math.Max(unitPriceFromMargin, basePrice.Value);
    }

    /// <summary>Costo total de la línea (insumos × cantidad de servicio).</summary>
    public static decimal LineCost(decimal unitCost, decimal serviceQuantity)
    {
        return unitCost * serviceQuantity;
    }

    /// <summary>
    /// Precio de línea: override manual (total) o (precio unitario tras margen y piso BasePrice) × cantidad.
    /// </summary>
    public static decimal LinePrice(
        decimal unitCost,
        decimal serviceQuantity,
        decimal marginPercent,
        decimal? basePrice,
        decimal? manualLinePriceOverride)
    {
        if (manualLinePriceOverride.HasValue)
            return manualLinePriceOverride.Value;

        var unitFromMargin = UnitPriceFromMargin(unitCost, marginPercent);
        var unit = ApplyBasePriceFloor(unitFromMargin, basePrice);
        return unit * serviceQuantity;
    }
}
