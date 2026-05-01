using FluentValidation;
using Presupuestos.Application.Dto.Pricing;

namespace Presupuestos.Application.Validation;

public class UpdatePricingRuleValidator : AbstractValidator<UpdatePricingRuleDto>
{
    public UpdatePricingRuleValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Type).NotEmpty().MaximumLength(32);
    }
}
