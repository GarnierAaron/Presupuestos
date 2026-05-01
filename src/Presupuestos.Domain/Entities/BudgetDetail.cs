namespace Presupuestos.Domain.Entities;

public class BudgetDetail
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal Quantity { get; set; }
    public decimal CalculatedCost { get; set; }
    public decimal CalculatedPrice { get; set; }

    /// <summary>Si tiene valor, sustituye el precio de línea calculado por margen (total de la línea).</summary>
    public decimal? ManualPriceOverride { get; set; }

    public Budget Budget { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
