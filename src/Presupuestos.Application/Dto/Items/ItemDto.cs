namespace Presupuestos.Application.Dto.Items;

public class ItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CostPerUnit { get; set; }
}

public class CreateItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CostPerUnit { get; set; }
}

public class UpdateItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal CostPerUnit { get; set; }
}
