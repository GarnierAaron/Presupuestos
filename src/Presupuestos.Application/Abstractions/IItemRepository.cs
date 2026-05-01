using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IItemRepository
{
    Task<bool> ExistsInTenantAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<Item?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Item>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);
    void Add(Item item);
    void Update(Item item);
    void Remove(Item item);
}
