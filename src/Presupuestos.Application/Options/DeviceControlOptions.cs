namespace Presupuestos.Application.Options;

/// <summary>
/// Control de dispositivos por usuario. Con <see cref="Enabled"/> false el comportamiento coincide con versiones anteriores.
/// </summary>
public class DeviceControlOptions
{
    public const string SectionName = "DeviceControl";

    /// <summary>Si es false, no se valida ni registra dispositivo.</summary>
    public bool Enabled { get; set; }

    /// <summary>Máximo de dispositivos activos por usuario (ignorado si <see cref="SingleDeviceOnly"/> es true).</summary>
    public int MaxDevicesPerUser { get; set; } = 5;

    /// <summary>Solo un dispositivo activo; conflicto según <see cref="WhenSecondDevice"/>.</summary>
    public bool SingleDeviceOnly { get; set; }

    /// <summary>Cuando hay más dispositivos activos que el máximo y llega uno nuevo: Reject | InvalidateOldest.</summary>
    public string WhenLimitExceeded { get; set; } = "InvalidateOldest";

    /// <summary>Con un solo dispositivo permitido y login desde otro id: Reject | Replace (invalida los demás).</summary>
    public string WhenSecondDevice { get; set; } = "Replace";

    /// <summary>Si está activo el control, las peticiones autenticadas deben enviar X-Device-Id.</summary>
    public bool RequireDeviceHeaderWhenEnabled { get; set; } = true;
}
