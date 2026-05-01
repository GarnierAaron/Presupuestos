using FluentValidation;
using Presupuestos.Application.Dto.Auth;

namespace Presupuestos.Application.Validation;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(320).EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
        When(x => !string.IsNullOrEmpty(x.DeviceId), () =>
        {
            RuleFor(x => x.DeviceId!).MaximumLength(200);
            RuleFor(x => x.DeviceName).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.DeviceName));
        });
    }
}
