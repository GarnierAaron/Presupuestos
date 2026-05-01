namespace Presupuestos.Application.Dto.Auth;

public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Nombre de la organización (tenant) que se crea junto al usuario administrador.</summary>
    public string TenantName { get; set; } = string.Empty;

    /// <summary>Opcional. Identificador del dispositivo si <c>DeviceControl:Enabled</c> y registro del primer terminal.</summary>
    public string? DeviceId { get; set; }

    public string? DeviceName { get; set; }
}

public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    /// <summary>Identificador estable del cliente; obligatorio cuando DeviceControl está habilitado.</summary>
    public string? DeviceId { get; set; }

    public string? DeviceName { get; set; }
}

public class RefreshRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class TokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAt { get; set; }
    public DateTimeOffset RefreshTokenExpiresAt { get; set; }
    public Guid UserId { get; set; }

    /// <summary>Null para sesión de super administrador global.</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Nombre legible del tenant (organización); null si no aplica.</summary>
    public string? TenantName { get; set; }
}
