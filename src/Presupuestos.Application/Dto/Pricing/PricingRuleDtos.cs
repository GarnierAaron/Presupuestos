namespace Presupuestos.Application.Dto.Pricing;

public class PricingRuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public string? Expression { get; set; }
    public int SortOrder { get; set; }
}

public class CreatePricingRuleDto
{
    public string Name { get; set; } = string.Empty;

    /// <summary>percentage | fixed | formula</summary>
    public string Type { get; set; } = string.Empty;

    public decimal? Value { get; set; }
    public string? Expression { get; set; }
    public int SortOrder { get; set; }
}

public class UpdatePricingRuleDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal? Value { get; set; }
    public string? Expression { get; set; }
    public int SortOrder { get; set; }
}

public class FlexiblePricingSettingsDto
{
    public bool FlexiblePricingEnabled { get; set; }
}
