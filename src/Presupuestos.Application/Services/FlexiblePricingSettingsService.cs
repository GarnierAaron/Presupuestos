using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Dto.Pricing;
using Presupuestos.Application.Tenancy;

namespace Presupuestos.Application.Services;

public class FlexiblePricingSettingsService
{
    private readonly ITenantRepository _tenants;
    private readonly ITenantContext _tenant;

    public FlexiblePricingSettingsService(ITenantRepository tenants, ITenantContext tenant)
    {
        _tenants = tenants;
        _tenant = tenant;
    }

    public async Task<FlexiblePricingSettingsDto> GetAsync(CancellationToken ct = default) =>
        new() { FlexiblePricingEnabled = await _tenants.IsFlexiblePricingEnabledAsync(_tenant.RequireTenantId(), ct) };

    public async Task SetAsync(FlexiblePricingSettingsDto dto, CancellationToken ct = default) =>
        await _tenants.SetFlexiblePricingEnabledAsync(_tenant.RequireTenantId(), dto.FlexiblePricingEnabled, ct);
}
