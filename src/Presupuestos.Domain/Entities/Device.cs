namespace Presupuestos.Domain.Entities;

/// <summary>
/// Dispositivo conocido del usuario (identificador enviado por el cliente).
/// </summary>
public class Device
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Identificador estable del cliente (ej. instalación de app).</summary>
    public string DeviceId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
    public DateTimeOffset LastLogin { get; set; }
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
