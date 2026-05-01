using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IPlanRepository
{
    Task<Plan?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Plan>> ListAsync(CancellationToken cancellationToken = default);
}
