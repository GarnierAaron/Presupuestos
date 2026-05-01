using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Dto.Subscription;
using Presupuestos.Application.Options;

namespace Presupuestos.Api.Controllers;

/// <summary>
/// Suscripciones del tenant y checkout Mercado Pago.
/// Rutas: POST /api/Subscriptions/create, GET /api/Subscriptions/me.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionAppService _subscriptions;

    public SubscriptionsController(ISubscriptionAppService subscriptions) =>
        _subscriptions = subscriptions;

    /// <summary>
    /// Inicia suscripción para la organización actual: plan Free se activa al instante; Pro/Premium devuelven URL de pago MP.
    /// </summary>
    /// <remarks>
    /// Request: <c>{"plan":"Pro"}</c>.
    /// Response Free: <c>{"subscriptionId":"...","checkoutUrl":null,"status":"Active"}</c>.
    /// Response pago: <c>{"subscriptionId":"...","checkoutUrl":"https://...","status":"Pending"}</c>.
    /// </remarks>
    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateSubscriptionResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CreateSubscriptionResponseDto>> Create(
        [FromBody] CreateSubscriptionRequestDto dto,
        [FromServices] IWebHostEnvironment env,
        [FromServices] IOptions<MercadoPagoOptions> mp,
        CancellationToken ct)
    {
        var useSandbox = env.IsDevelopment() && mp.Value.PreferSandboxInitPointInDevelopment;
        var result = await _subscriptions.CreateAsync(dto, useSandbox, ct);
        return Ok(result);
    }

    /// <summary>
    /// Estado de la suscripción del tenant (activa válida o última pendiente).
    /// </summary>
    /// <remarks>
    /// Response ejemplo: <c>{"plan":"Pro","status":"Active","endDate":"2026-01-01T00:00:00Z","startDate":"2025-12-02T00:00:00Z"}</c>.
    /// </remarks>
    [HttpGet("me")]
    [ProducesResponseType(typeof(MySubscriptionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MySubscriptionResponseDto>> Me(CancellationToken ct)
    {
        var me = await _subscriptions.GetMyAsync(ct);
        return me == null ? NotFound() : Ok(me);
    }
}
