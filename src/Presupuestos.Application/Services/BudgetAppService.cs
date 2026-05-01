using AutoMapper;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Tenancy;
using Presupuestos.Application.Dto.Budgets;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class BudgetAppService
{
    private readonly IBudgetRepository _budgets;
    private readonly IServiceRepository _services;
    private readonly IUserRepository _users;
    private readonly ITenantRepository _tenants;
    private readonly IPricingRuleRepository _pricingRules;
    private readonly IFlexiblePricingEngine _flexiblePricing;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenant;
    private readonly IMapper _mapper;

    public BudgetAppService(
        IBudgetRepository budgets,
        IServiceRepository services,
        IUserRepository users,
        ITenantRepository tenants,
        IPricingRuleRepository pricingRules,
        IFlexiblePricingEngine flexiblePricing,
        IUnitOfWork uow,
        ITenantContext tenant,
        IMapper mapper)
    {
        _budgets = budgets;
        _services = services;
        _users = users;
        _tenants = tenants;
        _pricingRules = pricingRules;
        _flexiblePricing = flexiblePricing;
        _uow = uow;
        _tenant = tenant;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BudgetDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await _budgets.ListAsync(_tenant.RequireTenantId(), ct);
        return list.Select(MapBudget).ToList();
    }

    public async Task<BudgetDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var b = await _budgets.GetByIdWithDetailsAsync(id, _tenant.RequireTenantId(), ct);
        return b == null ? null : MapBudget(b);
    }

    public async Task<BudgetDto> CreateAsync(CreateBudgetDto dto, CancellationToken ct = default)
    {
        if (dto.Lines.Count == 0)
            throw new InvalidOperationException("El presupuesto debe tener al menos una línea.");

        var tenant = await _tenants.GetByIdAsync(_tenant.RequireTenantId(), ct)
                     ?? throw new InvalidOperationException("Tenant no encontrado.");

        IReadOnlyList<PricingRule>? rules = null;
        if (tenant.FlexiblePricingEnabled)
            rules = await _pricingRules.ListOrderedForTenantAsync(_tenant.RequireTenantId(), ct);

        var useFlexiblePipeline = tenant.FlexiblePricingEnabled && rules is { Count: > 0 };

        decimal? userMargin = null;
        if (_tenant.UserId.HasValue)
        {
            var user = await _users.GetByIdAsync(_tenant.UserId.Value, _tenant.RequireTenantId(), ct);
            userMargin = user?.GlobalMarginPercent;
        }

        var budget = new Budget
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant.RequireTenantId(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByUserId = _tenant.UserId
        };

        decimal totalCost = 0;
        decimal totalPrice = 0;

        foreach (var line in dto.Lines)
        {
            var service = await _services.GetByIdWithItemsAsync(line.ServiceId, _tenant.RequireTenantId(), ct);
            if (service == null)
                throw new InvalidOperationException($"Servicio {line.ServiceId} no encontrado.");

            var unitCost = BudgetCalculator.ComputeUnitCost(service);
            var margin = BudgetCalculator.GetEffectiveMarginPercent(service, userMargin);
            var lineCost = BudgetCalculator.LineCost(unitCost, line.Quantity);

            decimal linePrice;
            if (useFlexiblePipeline)
            {
                if (line.ManualPriceOverride.HasValue)
                    linePrice = line.ManualPriceOverride.Value;
                else
                {
                    var unitSelling = _flexiblePricing.ComputeUnitSellingPrice(
                        unitCost,
                        line.Quantity,
                        rules!,
                        service.BasePrice);
                    linePrice = unitSelling * line.Quantity;
                }
            }
            else
            {
                linePrice = BudgetCalculator.LinePrice(
                    unitCost,
                    line.Quantity,
                    margin,
                    service.BasePrice,
                    line.ManualPriceOverride);
            }

            var detail = new BudgetDetail
            {
                Id = Guid.NewGuid(),
                BudgetId = budget.Id,
                ServiceId = service.Id,
                Quantity = line.Quantity,
                CalculatedCost = lineCost,
                CalculatedPrice = linePrice,
                ManualPriceOverride = line.ManualPriceOverride
            };

            budget.Details.Add(detail);
            totalCost += lineCost;
            totalPrice += linePrice;
        }

        budget.TotalCost = totalCost;
        budget.TotalPrice = totalPrice;

        _budgets.Add(budget);
        await _uow.SaveChangesAsync(ct);

        var loaded = await _budgets.GetByIdWithDetailsAsync(budget.Id, _tenant.RequireTenantId(), ct)
                     ?? throw new InvalidOperationException("Presupuesto no encontrado tras crear.");
        return MapBudget(loaded);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var b = await _budgets.GetByIdWithDetailsAsync(id, _tenant.RequireTenantId(), ct);
        if (b == null) return false;
        _budgets.Remove(b);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private BudgetDto MapBudget(Budget b)
    {
        var dto = _mapper.Map<BudgetDto>(b);
        dto.Details = b.Details.Select(d => new BudgetDetailDto
        {
            Id = d.Id,
            ServiceId = d.ServiceId,
            ServiceName = d.Service?.Name ?? string.Empty,
            Quantity = d.Quantity,
            CalculatedCost = d.CalculatedCost,
            CalculatedPrice = d.CalculatedPrice,
            ManualPriceOverride = d.ManualPriceOverride
        }).ToList();
        return dto;
    }
}
