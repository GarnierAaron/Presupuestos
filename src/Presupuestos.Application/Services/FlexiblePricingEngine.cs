using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Common.Exceptions;
using Presupuestos.Application.Pricing;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

/// <summary>
/// Ejemplo de pipeline (sin datos reales): material=100, qty=5
/// <list type="number">
/// <item><description>Regla 1 Percentage 30 → running = 100 × 1.30 = 130</description></item>
/// <item><description>Regla 2 Fixed 10 → running = 140</description></item>
/// <item><description>Regla 3 Formula "material * 1.5 + 50" → running = 100×1.5+50 = 200 (reemplaza tras pasos anteriores si la regla usa solo material/qty)</description></item>
/// </list>
/// En la práctica cada <see cref="PricingRuleType.Formula"/> usa <c>material</c>, el <c>price</c> acumulado y <c>qty</c>.
/// </summary>
public class FlexiblePricingEngine : IFlexiblePricingEngine
{
    public decimal ComputeUnitSellingPrice(
        decimal unitMaterialCost,
        decimal lineQuantity,
        IReadOnlyList<PricingRule> rules,
        decimal? basePriceFloor)
    {
        var ordered = rules.OrderBy(r => r.SortOrder).ThenBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
        var material = unitMaterialCost;
        decimal running = unitMaterialCost;

        foreach (var rule in ordered)
        {
            switch (rule.Type)
            {
                case PricingRuleType.Percentage:
                {
                    var p = rule.Value ?? 0m;
                    running *= 1m + p / 100m;
                    break;
                }
                case PricingRuleType.Fixed:
                    running += rule.Value ?? 0m;
                    break;
                case PricingRuleType.Formula:
                    if (string.IsNullOrWhiteSpace(rule.Expression))
                        throw new AuthException($"La regla \"{rule.Name}\" es tipo fórmula y no tiene expresión.");
                    running = SafeFormulaEvaluator.Evaluate(rule.Expression, material, running, lineQuantity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rule.Type), rule.Type, null);
            }
        }

        if (basePriceFloor.HasValue)
            running = Math.Max(running, basePriceFloor.Value);

        return running;
    }
}
