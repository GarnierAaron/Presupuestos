using System.Text.Json;
using Presupuestos.Application.Abstractions;
using Presupuestos.Application.Dto.App;
using Presupuestos.Application.Support;
using Presupuestos.Domain.Entities;

namespace Presupuestos.Application.Services;

public class RemoteAppConfigService : IRemoteAppConfigService
{
    private readonly IAppConfigRepository _repo;

    public RemoteAppConfigService(IAppConfigRepository repo) => _repo = repo;

    public async Task<RemoteAppStatusDto> GetStatusAsync(string? clientVersion, CancellationToken cancellationToken = default)
    {
        var config = await _repo.GetSingletonAsync(cancellationToken);
        config ??= DefaultConfig();

        var trimmedClient = string.IsNullOrWhiteSpace(clientVersion) ? null : clientVersion.Trim();

        var blockedList = ParseBlockedVersions(config.BlockedVersions);

        var dto = new RemoteAppStatusDto
        {
            Id = config.Id,
            AppEnabled = config.AppEnabled,
            MaintenanceMode = config.MaintenanceMode,
            ForceUpdate = config.ForceUpdate,
            ClientVersion = trimmedClient,
            MinimumVersion = config.MinimumVersion,
            BlockedVersions = blockedList,
            Message = config.Message ?? string.Empty,
            Status = "Ok",
            Blocked = false
        };

        if (!config.AppEnabled)
        {
            dto.Status = "Disabled";
            dto.Blocked = true;
            if (string.IsNullOrWhiteSpace(dto.Message))
                dto.Message = "La aplicación no está disponible en este momento.";
            return dto;
        }

        if (!string.IsNullOrEmpty(trimmedClient))
        {
            if (blockedList.Any(v => string.Equals(v, trimmedClient, StringComparison.OrdinalIgnoreCase)))
            {
                dto.Status = "VersionBlocked";
                dto.Blocked = true;
                if (string.IsNullOrWhiteSpace(dto.Message))
                    dto.Message = "Esta versión de la aplicación ya no está permitida.";
                return dto;
            }

            if (!string.IsNullOrWhiteSpace(config.MinimumVersion) &&
                AppVersionComparer.Compare(trimmedClient, config.MinimumVersion!) < 0)
            {
                dto.Status = "ForceUpdateRequired";
                dto.Blocked = true;
                dto.ForceUpdate = true;
                if (string.IsNullOrWhiteSpace(dto.Message))
                    dto.Message = $"Actualiza la aplicación. Versión mínima requerida: {config.MinimumVersion}.";
                return dto;
            }
        }

        if (config.ForceUpdate)
        {
            dto.ForceUpdate = true;
            dto.Status = "ForceUpdateRequired";
            dto.Blocked = true;
            if (string.IsNullOrWhiteSpace(dto.Message))
                dto.Message = "Es necesario actualizar la aplicación.";
            return dto;
        }

        if (config.MaintenanceMode)
        {
            dto.Status = "Maintenance";
        }

        return dto;
    }

    private static AppConfig DefaultConfig() =>
        new()
        {
            Id = Guid.Parse("00000000-0000-4000-8000-000000000001"),
            AppEnabled = true,
            MinimumVersion = null,
            BlockedVersions = "[]",
            ForceUpdate = false,
            MaintenanceMode = false,
            Message = string.Empty
        };

    private static IReadOnlyList<string> ParseBlockedVersions(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return Array.Empty<string>();
        try
        {
            var list = JsonSerializer.Deserialize<List<string>>(json);
            return list?.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList()
                   ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }
}
