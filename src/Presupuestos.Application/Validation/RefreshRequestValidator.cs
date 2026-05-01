using FluentValidation;
using Presupuestos.Application.Dto.Auth;

namespace Presupuestos.Application.Validation;

public class RefreshRequestValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().MaximumLength(2000);
    }
}
