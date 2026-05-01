namespace Presupuestos.Application.Dto.Budgets;

public class BudgetLineInputDto
{
    public Guid ServiceId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? ManualPriceOverride { get; set; }
}

public class CreateBudgetDto
{
    public IReadOnlyList<BudgetLineInputDto> Lines { get; set; } = Array.Empty<BudgetLineInputDto>();
}

public class BudgetDetailDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal CalculatedCost { get; set; }
    public decimal CalculatedPrice { get; set; }
    public decimal? ManualPriceOverride { get; set; }
}

public class BudgetDto
{
    public Guid Id { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public IReadOnlyList<BudgetDetailDto> Details { get; set; } = Array.Empty<BudgetDetailDto>();
}
