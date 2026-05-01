using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdWithDetailsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);
    void Add(Budget budget);
    void Update(Budget budget);
    void Remove(Budget budget);
}
