namespace Presupuestos.Domain.Entities;

/// <summary>Cantidad de insumo consumida por una unidad del servicio.</summary>
public class ServiceItem
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public Guid ItemId { get; set; }
    public decimal QuantityUsed { get; set; }

    public Service Service { get; set; } = null!;
    public Item Item { get; set; } = null!;
}
