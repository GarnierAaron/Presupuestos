namespace Presupuestos.Domain.Entities;

public class Budget
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Usuario que aporta el margen global usado en el cálculo (si aplica).</summary>
    public Guid? CreatedByUserId { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<BudgetDetail> Details { get; set; } = new List<BudgetDetail>();
}
