namespace Presupuestos.Application.Dto.Devices;

public class DeviceDto
{
    public Guid Id { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset LastLogin { get; set; }
    public bool IsActive { get; set; }
}
