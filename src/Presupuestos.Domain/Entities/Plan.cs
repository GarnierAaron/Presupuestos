namespace Presupuestos.Domain.Entities;

/// <summary>
/// Plan de suscripción (catálogo). La clave es <see cref="Name"/> (Free, Pro, Premium).
/// </summary>
public class Plan
{
    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
