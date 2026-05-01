using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IAppConfigRepository
{
    Task<AppConfig?> GetSingletonAsync(CancellationToken cancellationToken = default);
}
