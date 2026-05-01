using AutoMapper;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Dto.Services;
using Presupuestos.Application.Tenancy;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class ServiceCatalogService
{
    private readonly IServiceRepository _services;
    private readonly IItemRepository _items;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenant;
    private readonly IMapper _mapper;

    public ServiceCatalogService(
        IServiceRepository services,
        IItemRepository items,
        IUnitOfWork uow,
        ITenantContext tenant,
        IMapper mapper)
    {
        _services = services;
        _items = items;
        _uow = uow;
        _tenant = tenant;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ServiceDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await _services.ListAsync(_tenant.RequireTenantId(), ct);
        return list.Select(MapToDto).ToList();
    }

    public async Task<ServiceDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _services.GetByIdWithItemsAsync(id, _tenant.RequireTenantId(), ct);
        return e == null ? null : MapToDto(e);
    }

    public async Task<ServiceDto> CreateAsync(CreateServiceDto dto, CancellationToken ct = default)
    {
        await EnsureItemsBelongToTenant(dto.ServiceItems.Select(x => x.ItemId), ct);

        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant.RequireTenantId(),
            Name = dto.Name,
            BasePrice = dto.BasePrice,
            MarginPercent = dto.MarginPercent
        };

        foreach (var line in dto.ServiceItems)
        {
            service.ServiceItems.Add(new ServiceItem
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                ItemId = line.ItemId,
                QuantityUsed = line.QuantityUsed
            });
        }

        _services.Add(service);
        await _uow.SaveChangesAsync(ct);

        var loaded = await _services.GetByIdWithItemsAsync(service.Id, _tenant.RequireTenantId(), ct)
                     ?? throw new InvalidOperationException("Servicio no encontrado tras crear.");
        return MapToDto(loaded);
    }

    public async Task<ServiceDto?> UpdateAsync(Guid id, UpdateServiceDto dto, CancellationToken ct = default)
    {
        await EnsureItemsBelongToTenant(dto.ServiceItems.Select(x => x.ItemId), ct);

        var lines = dto.ServiceItems.Select(x => (x.ItemId, x.QuantityUsed)).ToList();
        var ok = await _services.TryReplaceServiceContentAsync(
            id,
            _tenant.RequireTenantId(),
            dto.Name.Trim(),
            dto.BasePrice,
            dto.MarginPercent,
            lines,
            ct);

        if (!ok) return null;

        var loaded = await _services.GetByIdWithItemsAsync(id, _tenant.RequireTenantId(), ct);
        return loaded == null ? null : MapToDto(loaded);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var service = await _services.GetByIdWithItemsAsync(id, _tenant.RequireTenantId(), ct);
        if (service == null) return false;
        _services.Remove(service);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private async Task EnsureItemsBelongToTenant(IEnumerable<Guid> itemIds, CancellationToken ct)
    {
        var ids = itemIds.Distinct().ToList();
        foreach (var itemId in ids)
        {
            if (!await _items.ExistsInTenantAsync(itemId, _tenant.RequireTenantId(), ct))
                throw new InvalidOperationException($"El insumo {itemId} no existe en el tenant.");
        }
    }

    private ServiceDto MapToDto(Service s)
    {
        var dto = _mapper.Map<ServiceDto>(s);
        dto.ServiceItems = s.ServiceItems.Select(si => new ServiceItemLineResponseDto
        {
            Id = si.Id,
            ItemId = si.ItemId,
            ItemName = si.Item?.Name ?? string.Empty,
            QuantityUsed = si.QuantityUsed
        }).ToList();
        return dto;
    }
}
