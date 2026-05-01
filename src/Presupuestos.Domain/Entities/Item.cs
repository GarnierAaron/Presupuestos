namespace Presupuestos.Domain.Entities;

/// <summary>Insumo genérico (tinta, papel, etc.).</summary>
public class Item
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CostPerUnit { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<ServiceItem> ServiceItems { get; set; } = new List<ServiceItem>();
}
