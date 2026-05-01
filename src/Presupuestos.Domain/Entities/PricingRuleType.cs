namespace Presupuestos.Domain.Entities;

public enum PricingRuleType
{
    /// <summary>Margen: precio u *= (1 + Value/100).</summary>
    Percentage = 0,

    /// <summary>Importe fijo por unidad: precio u += Value.</summary>
    Fixed = 1,

    /// <summary>Fórmula con variables material, price, qty (ver Expression).</summary>
    Formula = 2
}
