namespace Presupuestos.Domain.Entities;

/// <summary>
/// Regla de precio aplicada en orden sobre el precio unitario de venta (pipeline).
/// Solo se usa si el tenant tiene <see cref="Tenant.FlexiblePricingEnabled"/> y hay reglas.
/// </summary>
public class PricingRule
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public PricingRuleType Type { get; set; }

    /// <summary>Para Percentage (ej. 30 = 30%) o Fixed (importe por unidad).</summary>
    public decimal? Value { get; set; }

    /// <summary>Solo Type = Formula. Variables: material (costo unitario insumo), price (precio u acumulado), qty (cantidad línea).</summary>
    public string? Expression { get; set; }

    /// <summary>Orden ascendente de aplicación.</summary>
    public int SortOrder { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
