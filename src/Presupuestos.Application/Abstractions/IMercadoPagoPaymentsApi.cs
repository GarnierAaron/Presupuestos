using Presupuestos.Application.Dto.MercadoPago;

namespace Presupuestos.Application.Abstractions;

public interface IMercadoPagoPaymentsApi
{
    Task<MercadoPagoPreferenceResultDto> CreateCheckoutPreferenceAsync(
        MercadoPagoPreferenceRequestDto body,
        CancellationToken cancellationToken = default);

    Task<MercadoPagoPaymentDto?> GetPaymentAsync(long paymentId, CancellationToken cancellationToken = default);
}
