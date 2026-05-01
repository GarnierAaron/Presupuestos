namespace Presupuestos.Domain.Entities;

/// <summary>
/// Usuario del tenant (o super administrador global sin <see cref="TenantId"/>).
/// <see cref="GlobalMarginPercent"/> es el margen por defecto cuando el servicio no define uno propio.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    /// <summary>Null solo para <see cref="IsSuperAdmin"/> global; el resto pertenece a un tenant.</summary>
    public Guid? TenantId { get; set; }

    public string Email { get; set; } = string.Empty;

    /// <summary>Hash bcrypt de la contraseña.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>Acceso total al panel de administración central (sin tenant).</summary>
    public bool IsSuperAdmin { get; set; }

    /// <summary>Caducidad de la cuenta; null = sin caducidad.</summary>
    public DateTimeOffset? ExpirationDate { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    /// <summary>Margen global del usuario (% sobre costo). Nullable = usar 0% o política de tenant si la amplías.</summary>
    public decimal? GlobalMarginPercent { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Último login o refresh de token que emitió nuevo acceso.</summary>
    public DateTimeOffset? LastLogin { get; set; }

    public Tenant? Tenant { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Device> Devices { get; set; } = new List<Device>();
}
