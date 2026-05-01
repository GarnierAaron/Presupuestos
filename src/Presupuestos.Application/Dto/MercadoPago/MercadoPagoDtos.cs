using System.Text.Json.Serialization;

namespace Presupuestos.Application.Dto.MercadoPago;

public sealed class MercadoPagoPreferenceItemDto
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; } = 1;

    [JsonPropertyName("unit_price")]
    public decimal UnitPrice { get; set; }

    [JsonPropertyName("currency_id")]
    public string CurrencyId { get; set; } = "ARS";
}

public sealed class MercadoPagoPreferenceRequestDto
{
    [JsonPropertyName("items")]
    public List<MercadoPagoPreferenceItemDto> Items { get; set; } = new();

    [JsonPropertyName("external_reference")]
    public string ExternalReference { get; set; } = string.Empty;

    [JsonPropertyName("notification_url")]
    public string? NotificationUrl { get; set; }

    [JsonPropertyName("back_urls")]
    public MercadoPagoBackUrlsDto? BackUrls { get; set; }

    [JsonPropertyName("auto_return")]
    public string? AutoReturn { get; set; }
}

public sealed class MercadoPagoBackUrlsDto
{
    [JsonPropertyName("success")]
    public string? Success { get; set; }

    [JsonPropertyName("failure")]
    public string? Failure { get; set; }

    [JsonPropertyName("pending")]
    public string? Pending { get; set; }
}

public sealed class MercadoPagoPreferenceResultDto
{
    public string PreferenceId { get; set; } = string.Empty;

    public string InitPoint { get; set; } = string.Empty;

    public string? SandboxInitPoint { get; set; }
}

/// <summary>Subconjunto de la respuesta GET /v1/payments/{id} usado por el dominio.</summary>
public sealed class MercadoPagoPaymentDto
{
    public long Id { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? ExternalReference { get; set; }

    public string? PreferenceId { get; set; }
}
