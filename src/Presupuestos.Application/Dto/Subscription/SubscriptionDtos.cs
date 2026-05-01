namespace Presupuestos.Application.Dto.Subscription;

public sealed class CreateSubscriptionRequestDto
{
    /// <summary>Free, Pro o Premium (coincide con el plan en base de datos).</summary>
    public string Plan { get; set; } = string.Empty;
}

public sealed class CreateSubscriptionResponseDto
{
    public Guid SubscriptionId { get; set; }

    /// <summary>URL de checkout Mercado Pago. Vacío si el plan es gratuito (activación inmediata).</summary>
    public string? CheckoutUrl { get; set; }

    public string Status { get; set; } = string.Empty;
}

public sealed class MySubscriptionResponseDto
{
    public string Plan { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? EndDate { get; set; }

    public DateTime? StartDate { get; set; }
}
