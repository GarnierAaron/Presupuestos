using Presupuestos.Application.Dto.Devices;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Abstractions;

public interface IDeviceService
{
    /// <summary>Registro o actualización en login. Devuelve el Id de fila Device para asociar el refresh token.</summary>
    Task<Guid?> RegisterOrValidateForLoginAsync(User user, string? clientDeviceId, string? deviceName, CancellationToken cancellationToken = default);

    /// <summary>Tras registro de cuenta, opcionalmente registra el primer dispositivo.</summary>
    Task<Guid?> TryRegisterOnSignupAsync(User user, string? clientDeviceId, string? deviceName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceDto>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid userId, Guid deviceRecordId, CancellationToken cancellationToken = default);

    /// <summary>Revoca refresh tokens asociados al dispositivo.</summary>
    Task ForceLogoutDeviceAsync(Guid userId, Guid deviceRecordId, CancellationToken cancellationToken = default);

    /// <summary>Valida cabecera en peticiones autenticadas cuando el control está activo.</summary>
    Task ValidateRequestDeviceAsync(Guid userId, string? headerClientDeviceId, CancellationToken cancellationToken = default);
}
