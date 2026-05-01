using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Common.Exceptions;
using Presupuestos.Application.Dto.MercadoPago;
using Presupuestos.Application.Dto.Subscription;
using Presupuestos.Application.Options;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class SubscriptionAppService : ISubscriptionAppService
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IPlanRepository _plans;
    private readonly IMercadoPagoPaymentsApi _mercadoPago;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenant;
    private readonly MercadoPagoOptions _mp;

    public SubscriptionAppService(
        ISubscriptionRepository subscriptions,
        IPlanRepository plans,
        IMercadoPagoPaymentsApi mercadoPago,
        IUnitOfWork unitOfWork,
        ITenantContext tenant,
        IOptions<MercadoPagoOptions> mpOptions)
    {
        _subscriptions = subscriptions;
        _plans = plans;
        _mercadoPago = mercadoPago;
        _unitOfWork = unitOfWork;
        _tenant = tenant;
        _mp = mpOptions.Value;
    }

    public async Task<CreateSubscriptionResponseDto> CreateAsync(
        CreateSubscriptionRequestDto dto,
        bool useSandboxCheckoutUrl,
        CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenantId();

        var planKey = dto.Plan.Trim();
        if (planKey.Length == 0)
            throw new AuthException("El plan es obligatorio.");

        var plan = await _plans.GetByNameAsync(planKey, cancellationToken);
        if (plan == null)
            throw new AuthException("Plan no válido. Usá Free, Pro o Premium.");

        await _subscriptions.CancelPendingForTenantAsync(tenantId, cancellationToken);

        if (plan.Price <= 0)
        {
            await _subscriptions.CancelOtherActiveForTenantAsync(tenantId, Guid.Empty, cancellationToken);
            var freeId = Guid.NewGuid();
            var free = new Subscription
            {
                Id = freeId,
                TenantId = tenantId,
                PlanName = plan.Name,
                Status = SubscriptionStatuses.Active,
                StartDate = DateTime.UtcNow,
                EndDate = ComputeEndDateForPlan(plan, DateTime.UtcNow),
                CreatedAt = DateTime.UtcNow,
            };
            _subscriptions.Add(free);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new CreateSubscriptionResponseDto
            {
                SubscriptionId = freeId,
                CheckoutUrl = null,
                Status = SubscriptionStatuses.Active,
            };
        }

        if (string.IsNullOrWhiteSpace(_mp.NotificationUrl))
            throw new AuthException(
                "Configurá MercadoPago:NotificationUrl con la URL pública del webhook antes de cobrar planes de pago.");

        if (string.IsNullOrWhiteSpace(_mp.AccessToken))
            throw new AuthException("Configurá MercadoPago:AccessToken.");

        var subscriptionId = Guid.NewGuid();
        var preferenceBody = new MercadoPagoPreferenceRequestDto
        {
            Items =
            {
                new MercadoPagoPreferenceItemDto
                {
                    Title = $"Suscripción {plan.Name} — organización",
                    Quantity = 1,
                    UnitPrice = plan.Price,
                    CurrencyId = _mp.DefaultCurrencyId,
                },
            },
            ExternalReference = subscriptionId.ToString("D"),
            NotificationUrl = _mp.NotificationUrl.Trim(),
        };

        if (!string.IsNullOrWhiteSpace(_mp.BackUrlsSuccess) ||
            !string.IsNullOrWhiteSpace(_mp.BackUrlsFailure) ||
            !string.IsNullOrWhiteSpace(_mp.BackUrlsPending))
        {
            preferenceBody.BackUrls = new MercadoPagoBackUrlsDto
            {
                Success = _mp.BackUrlsSuccess,
                Failure = _mp.BackUrlsFailure,
                Pending = _mp.BackUrlsPending,
            };
            preferenceBody.AutoReturn = "approved";
        }

        MercadoPagoPreferenceResultDto mpResult;
        try
        {
            mpResult = await _mercadoPago.CreateCheckoutPreferenceAsync(preferenceBody, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new AuthException($"No se pudo crear la preferencia en Mercado Pago: {ex.Message}");
        }

        var checkoutUrl = useSandboxCheckoutUrl &&
                          _mp.PreferSandboxInitPointInDevelopment &&
                          !string.IsNullOrWhiteSpace(mpResult.SandboxInitPoint)
            ? mpResult.SandboxInitPoint
            : mpResult.InitPoint;

        if (string.IsNullOrWhiteSpace(checkoutUrl))
            throw new AuthException("Mercado Pago no devolvió URL de checkout.");

        var pending = new Subscription
        {
            Id = subscriptionId,
            TenantId = tenantId,
            PlanName = plan.Name,
            Status = SubscriptionStatuses.Pending,
            StartDate = DateTime.UtcNow,
            EndDate = null,
            PreferenceId = mpResult.PreferenceId,
            CreatedAt = DateTime.UtcNow,
        };
        _subscriptions.Add(pending);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSubscriptionResponseDto
        {
            SubscriptionId = subscriptionId,
            CheckoutUrl = checkoutUrl,
            Status = SubscriptionStatuses.Pending,
        };
    }

    public async Task<MySubscriptionResponseDto?> GetMyAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = RequireTenantId();
        var sub = await _subscriptions.GetCurrentForTenantAsync(tenantId, cancellationToken);
        if (sub == null)
            return null;

        return new MySubscriptionResponseDto
        {
            Plan = sub.PlanName,
            Status = sub.Status,
            EndDate = sub.EndDate,
            StartDate = sub.StartDate,
        };
    }

    public async Task ProcessMercadoPagoWebhookAsync(
        string? topic,
        string? idQuery,
        string? bodyJson,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        long? paymentId = TryResolvePaymentId(topic, idQuery, bodyJson, logger);
        if (paymentId == null)
        {
            logger.LogWarning("Webhook MP sin id de pago reconocible.");
            return;
        }

        MercadoPagoPaymentDto? payment;
        try
        {
            payment = await _mercadoPago.GetPaymentAsync(paymentId.Value, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo consultar el pago {PaymentId} en Mercado Pago.", paymentId);
            return;
        }

        if (payment == null)
        {
            logger.LogWarning("Pago {PaymentId} no encontrado en Mercado Pago.", paymentId);
            return;
        }

        var paymentIdStr = payment.Id.ToString();
        var existing = await _subscriptions.GetByExternalPaymentIdAsync(paymentIdStr, cancellationToken);
        if (existing != null && existing.Status == SubscriptionStatuses.Active)
        {
            logger.LogInformation("Webhook MP duplicado para pago ya aplicado {PaymentId}.", paymentId);
            return;
        }

        if (string.IsNullOrEmpty(payment.ExternalReference) ||
            !Guid.TryParse(payment.ExternalReference, out var subscriptionId))
        {
            logger.LogWarning("Pago {PaymentId} sin external_reference válido (subscription id).", paymentId);
            return;
        }

        var pending = await _subscriptions.GetByIdTrackedAsync(subscriptionId, cancellationToken);
        if (pending == null)
        {
            logger.LogWarning("Suscripción {SubscriptionId} no encontrada para el pago {PaymentId}.", subscriptionId, paymentId);
            return;
        }

        if (pending.TenantId == Guid.Empty)
        {
            logger.LogWarning("Suscripción inválida {SubscriptionId}.", subscriptionId);
            return;
        }

        var status = payment.Status.Trim().ToLowerInvariant();
        switch (status)
        {
            case "approved":
                await ApplyApprovedPaymentAsync(pending, paymentIdStr, logger, cancellationToken);
                break;
            case "rejected":
            case "cancelled":
            case "refunded":
            case "charged_back":
                if (pending.Status == SubscriptionStatuses.Pending)
                {
                    pending.Status = SubscriptionStatuses.Cancelled;
                    pending.ExternalPaymentId = paymentIdStr;
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                break;
            case "pending":
            case "in_process":
            case "in_mediation":
                logger.LogInformation("Pago {PaymentId} en estado {Status}; suscripción sigue pendiente.", paymentId, payment.Status);
                break;
            default:
                logger.LogWarning("Estado de pago MP no manejado: {Status}", payment.Status);
                break;
        }
    }

    private Guid RequireTenantId()
    {
        var tenantId = _tenant.TenantId;
        if (tenantId == null || tenantId == Guid.Empty)
            throw new ForbiddenAppException(
                "No hay organización en contexto. Operá con un usuario del tenant (JWT con tenant_id) o enviá el encabezado de integración.");
        return tenantId.Value;
    }

    private async Task ApplyApprovedPaymentAsync(
        Subscription pending,
        string paymentIdStr,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var plan = await _plans.GetByNameAsync(pending.PlanName, cancellationToken);
        if (plan == null)
        {
            logger.LogError("Plan {Plan} no existe para suscripción {Id}.", pending.PlanName, pending.Id);
            return;
        }

        if (pending.ExternalPaymentId == paymentIdStr && pending.Status == SubscriptionStatuses.Active)
            return;

        var activeSame = await _subscriptions.GetActiveByTenantAndPlanAsync(pending.TenantId, pending.PlanName, cancellationToken);
        if (activeSame != null && activeSame.Id != pending.Id)
        {
            var extendFrom = DateTime.UtcNow;
            if (activeSame.EndDate.HasValue)
                extendFrom = activeSame.EndDate.Value > DateTime.UtcNow ? activeSame.EndDate.Value : DateTime.UtcNow;

            var tracked = await _subscriptions.GetByIdTrackedAsync(activeSame.Id, cancellationToken);
            if (tracked != null)
            {
                tracked.EndDate = plan.DurationDays <= 0
                    ? null
                    : extendFrom.AddDays(plan.DurationDays);
                tracked.ExternalPaymentId = paymentIdStr;
            }

            pending.Status = SubscriptionStatuses.Cancelled;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Renovación: extendida suscripción {ActiveId}, checkout {PendingId} cancelado como absorbido.",
                activeSame.Id,
                pending.Id);
            return;
        }

        await _subscriptions.CancelOtherActiveForTenantAsync(pending.TenantId, pending.Id, cancellationToken);

        pending.Status = SubscriptionStatuses.Active;
        pending.StartDate = DateTime.UtcNow;
        pending.EndDate = ComputeEndDateForPlan(plan, DateTime.UtcNow);
        pending.ExternalPaymentId = paymentIdStr;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static DateTime? ComputeEndDateForPlan(Plan plan, DateTime fromUtc)
    {
        if (plan.Price <= 0 || plan.DurationDays <= 0)
            return null;
        return fromUtc.AddDays(plan.DurationDays);
    }

    private static long? TryResolvePaymentId(string? topic, string? idQuery, string? bodyJson, ILogger logger)
    {
        if (!string.IsNullOrEmpty(idQuery) &&
            long.TryParse(idQuery, out var fromQuery))
            return fromQuery;

        var t = topic?.Trim().ToLowerInvariant();
        if (t == "payment" && !string.IsNullOrEmpty(idQuery) && long.TryParse(idQuery, out var q2))
            return q2;

        if (string.IsNullOrWhiteSpace(bodyJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(bodyJson);
            var root = doc.RootElement;
            if (root.TryGetProperty("type", out var typeEl))
            {
                var type = typeEl.GetString()?.ToLowerInvariant();
                if (type == "payment" && root.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("id", out var idEl))
                {
                    if (idEl.ValueKind == JsonValueKind.String && long.TryParse(idEl.GetString(), out var idStr))
                        return idStr;
                    if (idEl.ValueKind == JsonValueKind.Number && idEl.TryGetInt64(out var idNum))
                        return idNum;
                }
            }

            if (root.TryGetProperty("topic", out var topicEl) &&
                string.Equals(topicEl.GetString(), "payment", StringComparison.OrdinalIgnoreCase) &&
                root.TryGetProperty("resource", out var resEl))
            {
                var resource = resEl.GetString();
                if (!string.IsNullOrEmpty(resource))
                {
                    var last = resource.TrimEnd('/').Split('/').LastOrDefault();
                    if (long.TryParse(last, out var fromResource))
                        return fromResource;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo parsear el cuerpo del webhook MP.");
        }

        return null;
    }
}
