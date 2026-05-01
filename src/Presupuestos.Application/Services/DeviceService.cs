using Microsoft.Extensions.Options;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Common.Exceptions;
using Presupuestos.Application.Dto.Devices;
using Presupuestos.Application.Options;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _devices;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IUnitOfWork _uow;
    private readonly DeviceControlOptions _opt;

    public DeviceService(
        IDeviceRepository devices,
        IRefreshTokenRepository refreshTokens,
        IUnitOfWork uow,
        IOptions<DeviceControlOptions> options)
    {
        _devices = devices;
        _refreshTokens = refreshTokens;
        _uow = uow;
        _opt = options.Value;
    }

    public async Task<Guid?> TryRegisterOnSignupAsync(User user, string? clientDeviceId, string? deviceName, CancellationToken cancellationToken = default)
    {
        if (!_opt.Enabled || string.IsNullOrWhiteSpace(clientDeviceId))
            return null;

        var trimmed = clientDeviceId.Trim();
        var existing = await _devices.FindByUserAndClientDeviceIdAsync(user.Id, trimmed, cancellationToken);
        if (existing != null)
        {
            existing.IsActive = true;
            existing.LastLogin = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(deviceName))
                existing.Name = deviceName.Trim();
            _devices.Update(existing);
            await _uow.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        await EnsureCanAddNewDeviceAsync(user.Id, cancellationToken);

        var device = new Device
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DeviceId = trimmed,
            Name = string.IsNullOrWhiteSpace(deviceName) ? "Dispositivo" : deviceName.Trim(),
            LastLogin = DateTimeOffset.UtcNow,
            IsActive = true
        };
        _devices.Add(device);
        await _uow.SaveChangesAsync(cancellationToken);
        return device.Id;
    }

    public async Task<Guid?> RegisterOrValidateForLoginAsync(User user, string? clientDeviceId, string? deviceName, CancellationToken cancellationToken = default)
    {
        if (!_opt.Enabled)
            return null;

        if (user.IsSuperAdmin)
            return null;

        if (string.IsNullOrWhiteSpace(clientDeviceId))
            throw new AuthException("Se requiere deviceId en el login cuando el control de dispositivos está activo.");

        var trimmed = clientDeviceId.Trim();
        var existing = await _devices.FindByUserAndClientDeviceIdAsync(user.Id, trimmed, cancellationToken);

        if (existing != null)
        {
            if (!existing.IsActive && string.Equals(_opt.WhenSecondDevice, "Reject", StringComparison.OrdinalIgnoreCase))
                throw new AuthException("Este dispositivo está desactivado.");

            if (_opt.SingleDeviceOnly && string.Equals(_opt.WhenSecondDevice, "Replace", StringComparison.OrdinalIgnoreCase))
                await DeactivateOthersAsync(user.Id, existing.Id, cancellationToken);

            existing.IsActive = true;
            existing.LastLogin = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(deviceName))
                existing.Name = deviceName.Trim();
            _devices.Update(existing);

            if (!_opt.SingleDeviceOnly)
                await TrimExcessDevicesAsync(user.Id, existing.Id, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        await EnsureCanAddNewDeviceAsync(user.Id, cancellationToken);

        var created = new Device
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DeviceId = trimmed,
            Name = string.IsNullOrWhiteSpace(deviceName) ? "Dispositivo" : deviceName.Trim(),
            LastLogin = DateTimeOffset.UtcNow,
            IsActive = true
        };

        _devices.Add(created);
        await _uow.SaveChangesAsync(cancellationToken);
        return created.Id;
    }

    private async Task EnsureCanAddNewDeviceAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (_opt.SingleDeviceOnly)
        {
            var actives = await _devices.ListActiveOrderedByLastLoginAsync(userId, cancellationToken);
            if (actives.Count == 0)
                return;

            if (string.Equals(_opt.WhenSecondDevice, "Reject", StringComparison.OrdinalIgnoreCase))
                throw new AuthException("Ya existe un dispositivo activo. No se permiten más.");

            foreach (var d in actives)
                await DeactivateAndRevokeInternalAsync(d, cancellationToken);

            return;
        }

        var max = Math.Max(1, _opt.MaxDevicesPerUser);
        var count = await _devices.CountActiveForUserAsync(userId, cancellationToken);
        if (count < max)
            return;

        if (string.Equals(_opt.WhenLimitExceeded, "Reject", StringComparison.OrdinalIgnoreCase))
            throw new AuthException($"Has alcanzado el máximo de {_opt.MaxDevicesPerUser} dispositivos.");

        var ordered = await _devices.ListActiveOrderedByLastLoginAsync(userId, cancellationToken);
        var oldest = ordered.FirstOrDefault();
        if (oldest != null)
            await DeactivateAndRevokeInternalAsync(oldest, cancellationToken);
    }

    private async Task DeactivateOthersAsync(Guid userId, Guid keepDeviceId, CancellationToken cancellationToken)
    {
        var all = await _devices.ListForUserAsync(userId, cancellationToken);
        foreach (var d in all.Where(x => x.Id != keepDeviceId && x.IsActive))
            await DeactivateAndRevokeInternalAsync(d, cancellationToken);
    }

    private async Task TrimExcessDevicesAsync(Guid userId, Guid keepDeviceId, CancellationToken cancellationToken)
    {
        var max = Math.Max(1, _opt.MaxDevicesPerUser);
        var actives = (await _devices.ListActiveOrderedByLastLoginAsync(userId, cancellationToken)).ToList();
        var others = actives.Where(d => d.Id != keepDeviceId).OrderBy(d => d.LastLogin).ToList();
        var total = 1 + others.Count;
        if (total <= max)
            return;

        if (string.Equals(_opt.WhenLimitExceeded, "Reject", StringComparison.OrdinalIgnoreCase))
            throw new AuthException($"Has alcanzado el máximo de {_opt.MaxDevicesPerUser} dispositivos.");

        var toDrop = total - max;
        foreach (var victim in others.Take(toDrop))
            await DeactivateAndRevokeInternalAsync(victim, cancellationToken);
    }

    private async Task DeactivateAndRevokeInternalAsync(Device device, CancellationToken cancellationToken)
    {
        device.IsActive = false;
        _devices.Update(device);
        await _refreshTokens.RevokeAllForDeviceAsync(device.Id, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeviceDto>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var list = await _devices.ListForUserAsync(userId, cancellationToken);
        return list.Select(d => new DeviceDto
        {
            Id = d.Id,
            DeviceId = d.DeviceId,
            Name = d.Name,
            LastLogin = d.LastLogin,
            IsActive = d.IsActive
        }).ToList();
    }

    public async Task DeactivateAsync(Guid userId, Guid deviceRecordId, CancellationToken cancellationToken = default)
    {
        var d = await _devices.GetByIdForUserAsync(deviceRecordId, userId, cancellationToken)
                  ?? throw new AuthException("Dispositivo no encontrado.");
        await DeactivateAndRevokeInternalAsync(d, cancellationToken);
    }

    public async Task ForceLogoutDeviceAsync(Guid userId, Guid deviceRecordId, CancellationToken cancellationToken = default)
    {
        var d = await _devices.GetByIdForUserAsync(deviceRecordId, userId, cancellationToken)
                  ?? throw new AuthException("Dispositivo no encontrado.");
        await _refreshTokens.RevokeAllForDeviceAsync(d.Id, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task ValidateRequestDeviceAsync(Guid userId, string? headerClientDeviceId, CancellationToken cancellationToken = default)
    {
        if (!_opt.Enabled || !_opt.RequireDeviceHeaderWhenEnabled)
            return;

        if (string.IsNullOrWhiteSpace(headerClientDeviceId))
            throw new ForbiddenAppException("Falta la cabecera X-Device-Id para este usuario.");

        var trimmed = headerClientDeviceId.Trim();
        var device = await _devices.FindByUserAndClientDeviceIdAsync(userId, trimmed, cancellationToken);
        if (device == null || !device.IsActive)
            throw new ForbiddenAppException("Dispositivo no registrado o inactivo.");
    }
}
