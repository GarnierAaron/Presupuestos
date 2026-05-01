using FluentValidation;
using Presupuestos.Application.Dto.Pricing;

namespace Presupuestos.Application.Validation;

public class CreatePricingRuleValidator : AbstractValidator<CreatePricingRuleDto>
{
    public CreatePricingRuleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(32);
    }
}
