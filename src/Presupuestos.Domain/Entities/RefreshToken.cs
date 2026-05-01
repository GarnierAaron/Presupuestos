namespace Presupuestos.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Hash del token (p. ej. SHA256 en Base64); no guardar el token en claro.</summary>
    public string Token { get; set; } = string.Empty;

    public DateTimeOffset Expiration { get; set; }

    /// <summary>Sesión asociada a un dispositivo concreto (si el control de dispositivos está activo).</summary>
    public Guid? DeviceRecordId { get; set; }

    public User User { get; set; } = null!;
    public Device? Device { get; set; }
}
