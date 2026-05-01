namespace Presupuestos.Application.Options;

/// <summary>
/// Controla si el middleware exige suscripción activa para rutas de negocio.
/// </summary>
public class SubscriptionAccessOptions
{
    public const string SectionName = "SubscriptionAccess";

    /// <summary>Si es false, no se bloquea por suscripción (útil en desarrollo).</summary>
    public bool Enforce { get; set; } = true;
}
