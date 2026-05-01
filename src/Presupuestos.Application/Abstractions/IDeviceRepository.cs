using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IDeviceRepository
{
    Task<Device?> FindByUserAndClientDeviceIdAsync(Guid userId, string clientDeviceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Device>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Device?> GetByIdForUserAsync(Guid deviceRecordId, Guid userId, CancellationToken cancellationToken = default);

    Task<int> CountActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Activos ordenados por <see cref="Device.LastLogin"/> ascendente (más antiguo primero).</summary>
    Task<IReadOnlyList<Device>> ListActiveOrderedByLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);

    void Add(Device device);
    void Update(Device device);
}
