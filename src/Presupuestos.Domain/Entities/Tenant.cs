namespace Presupuestos.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Si es true, los presupuestos usan <see cref="PricingRule"/> en lugar de margen/BasePrice clásico.</summary>
    public bool FlexiblePricingEnabled { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Item> Items { get; set; } = new List<Item>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<PricingRule> PricingRules { get; set; } = new List<PricingRule>();
}
