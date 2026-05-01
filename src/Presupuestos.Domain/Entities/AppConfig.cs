namespace Presupuestos.Domain.Entities;

/// <summary>
/// Configuración global remota de la app cliente (kill switch, versiones, mantenimiento).
/// Convención: una sola fila (singleton).</summary>
public class AppConfig
{
    public Guid Id { get; set; }

    public bool AppEnabled { get; set; } = true;

    /// <summary>Versión mínima admitida (formato tipo semver o Major.Minor.Patch).</summary>
    public string? MinimumVersion { get; set; }

    /// <summary>JSON con array de strings, ej. ["1.0.0","2.0.1"].</summary>
    public string BlockedVersions { get; set; } = "[]";

    /// <summary>Fuerza actualización para todos los clientes que consultan.</summary>
    public bool ForceUpdate { get; set; }

    public bool MaintenanceMode { get; set; }

    /// <summary>Mensaje para mantenimiento u otros avisos.</summary>
    public string Message { get; set; } = string.Empty;
}
