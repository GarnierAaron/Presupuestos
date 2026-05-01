namespace Presupuestos.Domain.Entities;

/// <summary>
/// Suscripción de la organización (<see cref="Tenant"/>). Patrón B2B habitual: un pago cubre al tenant.
/// El estado se actualiza vía webhook de Mercado Pago (planes de pago) o al instante para Free.
/// </summary>
public class Subscription
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Coincide con <see cref="Plan.Name"/> (Free, Pro, Premium).</summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>Pending, Active, Cancelled, Expired.</summary>
    public string Status { get; set; } = SubscriptionStatuses.Pending;

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>Id del pago en Mercado Pago (stringificado).</summary>
    public string? ExternalPaymentId { get; set; }

    public string? PreferenceId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Plan Plan { get; set; } = null!;
}

public static class SubscriptionStatuses
{
    public const string Pending = "Pending";
    public const string Active = "Active";
    public const string Cancelled = "Cancelled";
    public const string Expired = "Expired";
}
