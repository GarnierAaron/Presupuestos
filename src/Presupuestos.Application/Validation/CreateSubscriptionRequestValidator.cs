using FluentValidation;
using Presupuestos.Application.Dto.Subscription;

namespace Presupuestos.Application.Validation;

public sealed class CreateSubscriptionRequestValidator : AbstractValidator<CreateSubscriptionRequestDto>
{
    public CreateSubscriptionRequestValidator()
    {
        RuleFor(x => x.Plan).NotEmpty().MaximumLength(50);
    }
}
