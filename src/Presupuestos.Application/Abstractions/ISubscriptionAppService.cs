using Microsoft.Extensions.Logging;
using Presupuestos.Application.Dto.Subscription;

namespace Presupuestos.Application.Abstractions;

public interface ISubscriptionAppService
{
    Task<CreateSubscriptionResponseDto> CreateAsync(
        CreateSubscriptionRequestDto dto,
        bool useSandboxCheckoutUrl,
        CancellationToken cancellationToken = default);

    /// <summary>Estado de suscripción del tenant actual (contexto HTTP / JWT).</summary>
    Task<MySubscriptionResponseDto?> GetMyAsync(CancellationToken cancellationToken = default);

    /// <summary>Procesa notificación MP; idempotente. Errores se registran; no relanzar para evitar reintentos infinitos de MP.</summary>
    Task ProcessMercadoPagoWebhookAsync(
        string? topic,
        string? idQuery,
        string? bodyJson,
        ILogger logger,
        CancellationToken cancellationToken = default);
}
