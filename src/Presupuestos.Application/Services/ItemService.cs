using AutoMapper;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Tenancy;
using Presupuestos.Application.Dto.Items;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class ItemService
{
    private readonly IItemRepository _items;
    private readonly IUnitOfWork _uow;
    private readonly ITenantContext _tenant;
    private readonly IMapper _mapper;

    public ItemService(IItemRepository items, IUnitOfWork uow, ITenantContext tenant, IMapper mapper)
    {
        _items = items;
        _uow = uow;
        _tenant = tenant;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ItemDto>> ListAsync(CancellationToken ct = default)
    {
        var list = await _items.ListAsync(_tenant.RequireTenantId(), ct);
        return _mapper.Map<IReadOnlyList<ItemDto>>(list);
    }

    public async Task<ItemDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _items.GetByIdAsync(id, _tenant.RequireTenantId(), ct);
        return e == null ? null : _mapper.Map<ItemDto>(e);
    }

    public async Task<ItemDto> CreateAsync(CreateItemDto dto, CancellationToken ct = default)
    {
        var e = new Item
        {
            Id = Guid.NewGuid(),
            TenantId = _tenant.RequireTenantId(),
            Name = dto.Name,
            Unit = dto.Unit,
            CostPerUnit = dto.CostPerUnit
        };
        _items.Add(e);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ItemDto>(e);
    }

    public async Task<ItemDto?> UpdateAsync(Guid id, UpdateItemDto dto, CancellationToken ct = default)
    {
        var e = await _items.GetByIdAsync(id, _tenant.RequireTenantId(), ct);
        if (e == null) return null;
        e.Name = dto.Name;
        e.Unit = dto.Unit;
        e.CostPerUnit = dto.CostPerUnit;
        _items.Update(e);
        await _uow.SaveChangesAsync(ct);
        return _mapper.Map<ItemDto>(e);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _items.GetByIdAsync(id, _tenant.RequireTenantId(), ct);
        if (e == null) return false;
        _items.Remove(e);
        await _uow.SaveChangesAsync(ct);
        return true;
    }
}
