using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Tenancy;
using Presupuestos.Application.Common.Exceptions;
using Presupuestos.Application.Dto.Pricing;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class PricingRuleService
{
    private readonly IPricingRuleRepository _rules;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenant;

    public PricingRuleService(
        IPricingRuleRepository rules,
        IUnitOfWork uow,
        ITenantContext tenant)
    {
        _rules = rules;
        _uow = uow;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<PricingRuleDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await _rules.ListOrderedForTenantAsync(_tenant.RequireTenantId(), ct);
        return list.Select(MapDto).ToList();
    }

    public async Task<PricingRuleDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _rules.GetByIdAsync(id, _tenant.RequireTenantId(), ct);
        return e == null ? null : MapDto(e);
    }

    public async Task<PricingRuleDto> CreateAsync(CreatePricingRuleDto dto, CancellationToken ct = default)
    {
        var type = ParseType(dto.Type);
        ValidateRule(type, dto.Value, dto.Expression);

        var e = new PricingRule
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant.RequireTenantId(),
            Name = dto.Name.Trim(),
            Type = type,
            Value = dto.Value,
            Expression = string.IsNullOrWhiteSpace(dto.Expression) ? null : dto.Expression.Trim(),
            SortOrder = dto.SortOrder
        };
        _rules.Add(e);
        await _uow.SaveChangesAsync(ct);

        var loaded = await _rules.GetByIdAsync(e.Id, _tenant.RequireTenantId(), ct)
                     ?? throw new InvalidOperationException("Regla no encontrada tras crear.");
        return MapDto(loaded);
    }

    public async Task<PricingRuleDto?> UpdateAsync(Guid id, UpdatePricingRuleDto dto, CancellationToken ct = default)
    {
        var e = await _rules.GetByIdAsync(id, _tenant.RequireTenantId(), ct);
        if (e == null) return null;

        var type = ParseType(dto.Type);
        ValidateRule(type, dto.Value, dto.Expression);

        e.Name = dto.Name.Trim();
        e.Type = type;
        e.Value = dto.Value;
        e.Expression = string.IsNullOrWhiteSpace(dto.Expression) ? null : dto.Expression.Trim();
        e.SortOrder = dto.SortOrder;
        _rules.Update(e);
        await _uow.SaveChangesAsync(ct);

        var loaded = await _rules.GetByIdAsync(id, _tenant.RequireTenantId(), ct);
        return loaded == null ? null : MapDto(loaded);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _rules.GetByIdAsync(id, _tenant.RequireTenantId(), ct);
        if (e == null) return false;
        _rules.Remove(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private static PricingRuleType ParseType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new AuthException("El tipo de regla es obligatorio.");
        if (!Enum.TryParse<PricingRuleType>(raw.Trim(), ignoreCase: true, out var t))
            throw new AuthException("Tipo inválido. Use: Percentage, Fixed o Formula.");
        return t;
    }

    private static void ValidateRule(PricingRuleType type, decimal? value, string? expression)
    {
        switch (type)
        {
            case PricingRuleType.Percentage:
            case PricingRuleType.Fixed:
                break;
            case PricingRuleType.Formula:
                if (string.IsNullOrWhiteSpace(expression))
                    throw new AuthException("Las reglas tipo Formula requieren Expression.");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    private static PricingRuleDto MapDto(PricingRule r) =>
        new()
        {
            Id = r.Id,
            Name = r.Name,
            Type = r.Type.ToString(),
            Value = r.Value,
            Expression = r.Expression,
            SortOrder = r.SortOrder
        };
}
