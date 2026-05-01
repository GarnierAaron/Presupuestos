namespace Presupuestos.Domain.Entities;

/// <summary>Servicio ofrecido (corte, impresión, etc.).</summary>
public class Service
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Precio base opcional por unidad; si existe, el precio final por unidad no será menor (tras aplicar margen).</summary>
    public decimal? BasePrice { get; set; }

    /// <summary>Margen % sobre costo para este servicio. Si es null, se usa el margen global del usuario en el presupuesto.</summary>
    public decimal? MarginPercent { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
    public ICollection<BudgetDetail> BudgetDetails { get; set; } = new List<BudgetDetail>();
}
