using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<Service?> GetByIdWithItemsAsync(Guid id, Guid tenantId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> ListAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Borra todas las líneas del servicio (SQL directo: no carga entidades y no enlaza al Service rastreado).</summary>
    Task DeleteItemsForServiceAsync(Guid serviceId, Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Actualiza escalares del servicio, reemplaza líneas y confirma en una transacción (sin <see cref="Service"/> rastreado).
    /// </summary>
    Task<bool> TryReplaceServiceContentAsync(
        Guid serviceId,
        Guid tenantId,
        string name,
        decimal? basePrice,
        decimal? marginPercent,
        IReadOnlyList<(Guid ItemId, decimal QuantityUsed)> lines,
        CancellationToken cancellationToken = default);

    void Add(Service service);

    void Update(Service service);

    void Remove(Service service);
}
