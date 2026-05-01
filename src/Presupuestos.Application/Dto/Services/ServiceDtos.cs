namespace Presupuestos.Application.Dto.Services;

public class ServiceItemLineDto
{
    public Guid ItemId { get; set; }
    public decimal QuantityUsed { get; set; }
}

public class ServiceItemLineResponseDto
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal QuantityUsed { get; set; }
}

public class ServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? BasePrice { get; set; }
    public decimal? MarginPercent { get; set; }
    public IReadOnlyList<ServiceItemLineResponseDto> ServiceItems { get; set; } = Array.Empty<ServiceItemLineResponseDto>();
}

public class CreateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal? BasePrice { get; set; }
    public decimal? MarginPercent { get; set; }
    public IReadOnlyList<ServiceItemLineDto> ServiceItems { get; set; } = Array.Empty<ServiceItemLineDto>();
}

public class UpdateServiceDto
{
    public string Name { get; set; } = string.Empty;
    public decimal? BasePrice { get; set; }
    public decimal? MarginPercent { get; set; }
    public IReadOnlyList<ServiceItemLineDto> ServiceItems { get; set; } = Array.Empty<ServiceItemLineDto>();
}
