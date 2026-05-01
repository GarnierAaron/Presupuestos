using Presupuestos.Application.Dto.App;

namespace Presupuestos.Application.Abstractions;

public interface IRemoteAppConfigService
{
    Task<RemoteAppStatusDto> GetStatusAsync(string? clientVersion, CancellationToken cancellationToken = default);
}
