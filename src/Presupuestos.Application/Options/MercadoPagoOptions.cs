namespace Presupuestos.Application.Options;

public class MercadoPagoOptions
{
    public const string SectionName = "MercadoPago";

    /// <summary>Access token de la aplicación (server-side).</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>Ej. https://api.mercadopago.com</summary>
    public string BaseUrl { get; set; } = "https://api.mercadopago.com";

    /// <summary>URL pública del webhook (POST /api/webhooks/mercadopago).</summary>
    public string NotificationUrl { get; set; } = string.Empty;

    public string DefaultCurrencyId { get; set; } = "ARS";

    /// <summary>Si true y el host es Development, se usa sandbox_init_point cuando exista.</summary>
    public bool PreferSandboxInitPointInDevelopment { get; set; } = true;

    public string? BackUrlsSuccess { get; set; }
    public string? BackUrlsFailure { get; set; }
    public string? BackUrlsPending { get; set; }
}
