using Microsoft.EntityFrameworkCore;
using Presupuestos.Application.Abstractions;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Infrastructure.Persistence;

public class DeviceRepository : IDeviceRepository
{
    private readonly AppDbContext _db;

    public DeviceRepository(AppDbContext db) => _db = db;

    public async Task<Device?> FindByUserAndClientDeviceIdAsync(Guid userId, string clientDeviceId, CancellationToken cancellationToken = default) =>
        await _db.Devices.FirstOrDefaultAsync(
            d => d.UserId == userId && d.DeviceId == clientDeviceId,
            cancellationToken);

    public async Task<IReadOnlyList<Device>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Devices.AsNoTracking()
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastLogin)
            .ToListAsync(cancellationToken);

    public async Task<Device?> GetByIdForUserAsync(Guid deviceRecordId, Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Devices.FirstOrDefaultAsync(d => d.Id == deviceRecordId && d.UserId == userId, cancellationToken);

    public async Task<int> CountActiveForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Devices.CountAsync(d => d.UserId == userId && d.IsActive, cancellationToken);

    public Task<int> CountForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _db.Devices.CountAsync(d => d.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Device>> ListActiveOrderedByLastLoginAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _db.Devices.AsNoTracking()
            .Where(d => d.UserId == userId && d.IsActive)
            .OrderBy(d => d.LastLogin)
            .ToListAsync(cancellationToken);

    public void Add(Device device) => _db.Devices.Add(device);

    public void Update(Device device) => _db.Devices.Update(device);
}
