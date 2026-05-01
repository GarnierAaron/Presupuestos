using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presupuestos.Application.Abstractions;

namespace Presupuestos.Api.Controllers;

/// <summary>
/// Webhooks de proveedores de pago (sin JWT). Mercado Pago notifica aquí; el pago se valida contra la API de MP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly ISubscriptionAppService _subscriptions;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(ISubscriptionAppService subscriptions, ILogger<WebhooksController> logger)
    {
        _subscriptions = subscriptions;
        _logger = logger;
    }

    /// <summary>POST /api/webhooks/mercadopago — topic/id por query y/o JSON con type payment.</summary>
    [HttpPost("mercadopago")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MercadoPago(
        [FromQuery] string? topic,
        [FromQuery] string? id,
        CancellationToken ct)
    {
        string? bodyJson = null;
        if (Request.HasFormContentType)
        {
            topic ??= Request.Form["topic"].FirstOrDefault();
            id ??= Request.Form["id"].FirstOrDefault();
        }
        else
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body, leaveOpen: true);
            var raw = await reader.ReadToEndAsync(ct);
            Request.Body.Position = 0;
            if (!string.IsNullOrWhiteSpace(raw))
                bodyJson = raw;
        }

        await _subscriptions.ProcessMercadoPagoWebhookAsync(topic, id, bodyJson, _logger, ct);
        return Ok();
    }
}
