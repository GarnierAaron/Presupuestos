namespace Presupuestos.Application.Dto.App;

/// <summary>Respuesta de GET /api/app-config para que la app cliente aplique kill switch y políticas.</summary>
public class RemoteAppStatusDto
{
    public Guid Id { get; set; }

    public bool AppEnabled { get; set; }

    public bool MaintenanceMode { get; set; }

    public bool ForceUpdate { get; set; }

    /// <summary>La app no debe continuar (kill switch, versión bloqueada o muy antigua).</summary>
    public bool Blocked { get; set; }

    /// <summary>Versión del cliente evaluada (query o cabecera).</summary>
    public string? ClientVersion { get; set; }

    public string? MinimumVersion { get; set; }

    public IReadOnlyList<string> BlockedVersions { get; set; } = Array.Empty<string>();

    /// <summary>Mensaje configurado (mantenimiento, etc.).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Ok | Disabled | Maintenance | VersionBlocked | ForceUpdateRequired</summary>
    public string Status { get; set; } = "Ok";
}
