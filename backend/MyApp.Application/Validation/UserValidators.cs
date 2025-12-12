using FluentValidation;
using MyApp.Application.DTOs;

namespace MyApp.Application.Validation;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID 不可為 0");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("角色必填")
            .Must(role => role == "Admin" || role == "User")
            .WithMessage("角色必須是 Admin 或 User");

        // 密碼選填
        RuleFor(x => x.Password)
            .MinimumLength(6)
            .When(x => !string.IsNullOrWhiteSpace(x.Password))
            .WithMessage("密碼至少 6 碼");

        RuleFor(x => x.IsActive)
            .NotNull()
            .WithMessage("狀態必填");
    }
}
